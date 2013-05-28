using Outlook = Microsoft.Office.Interop.Outlook;
using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using Navigate.Quartz;
using Navigate.Controllers;

namespace Navigate.Services
{
    public class OutlookItemImportService
    {
        private NavigateDb dataContext;

        private UserProfile CurrentUser;

        public DateTime IntervalStart;

        public DateTime IntervalEnd;

        private ReminderScheduler scheduler = new ReminderScheduler();

        public OutlookItemImportService(NavigateDb _dataContext, UserProfile _currentUser)
        {
            this.dataContext = _dataContext;
            this.CurrentUser = _currentUser;
        }
        /// <summary>
        /// Imports the items in Outlook calendar folder of current user
        /// </summary>
        public ServiceResult<OutlookItemImportServiceResult> ImportOutlookCalendarItems()
        {
            Outlook.Application outlookApp = null;
            Outlook.NameSpace mapiNamespace = null;
            Outlook.MAPIFolder CalendarFolder = null;
            Outlook.Items outlookCalendarItems = null;

            var messages = new List<Message>();

            // try to initialize Outlook API and log on
            try
            {
                outlookApp = new Outlook.Application();
                mapiNamespace = outlookApp.GetNamespace("MAPI");
                CalendarFolder = mapiNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
                mapiNamespace.Logon(Missing.Value, Missing.Value, true, true);
            }
            catch (Exception ex)
            {
                messages.Add(new Message() { Text = string.Format("Neparedzēta kļūda. ", ex.Message), Severity = MessageSeverity.Error });
            }

            //build a filter from user inputs
            String filter = String.Format("([Start] >= '{0:g}') AND ([End] <= '{1:g}')", IntervalStart, IntervalEnd);
     
            //get the filtered Outlook items including the recurring items and sort them to expand the recurrences automatically
            outlookCalendarItems = CalendarFolder.Items.Restrict(filter);
            outlookCalendarItems.Sort("[Start]");
            outlookCalendarItems.IncludeRecurrences = true;

            //if there are no items in the calendar folder within the specified interval, return and notify the user
            if (outlookCalendarItems.GetFirst() == null)
            {
                var message = "Norādītajā laika periodā Jūsu Outlook kalendārā netika atrasts neviens uzdevums";
                return new ServiceResult<OutlookItemImportServiceResult>()
                {
                    Data = OutlookItemImportServiceResult.NotImported,
                    Messages = new Message[] { new Message() { Text = message, Severity = MessageSeverity.Info } }
                };
            }

            //iterate through each returned Outlook calendar item and process it
            foreach (Outlook.AppointmentItem item in outlookCalendarItems)
            {
                var existingWorkItem = this.dataContext.WorkItems.Where(o => o.OutlookEntryId != null && o.OutlookEntryId == item.EntryID).FirstOrDefault();

                //if item is not recurring, create a new work item or update an exisiting one 
                if (!item.IsRecurring)
                {
                    if (existingWorkItem == null)
                    {
                        var workItem = CreateNonRecurringWorkItem(item);
                        this.dataContext.WorkItems.Add(workItem);
                        SetReminderData(scheduler, workItem);
                        scheduler.ScheduleReminder();
                    }
                    else
                    {
                        if (existingWorkItem.UpdatedAt <= item.LastModificationTime)
                        {
                            UpdateNonRecurringWorkItem(existingWorkItem, item);
                            if (existingWorkItem.Reminder != Reminder.None && existingWorkItem.EndDateTime > DateTime.Now)
                            {
                                SetReminderData(scheduler, existingWorkItem);
                                scheduler.RescheduleReminder();
                            }
                        }
                    }
                    this.dataContext.SaveChanges();
                }
                else if (item.IsRecurring)
                {
                    Outlook.RecurrencePattern recurrencePattern = item.GetRecurrencePattern();
                    RecurrenceType recurrenceType = 0;

                    //Get all the exceptions in the series of this recurring item, e.g. items which belong to a series of occurrences, but whose properties have been changed
                    List<Outlook.Exception> exceptions = new List<Outlook.Exception>();
                    foreach (Outlook.Exception exception in recurrencePattern.Exceptions)
                    {
                        exceptions.Add(exception);
                    }

                    int recurTypeId = (int)recurrencePattern.RecurrenceType;
                    bool hasEndDate;
                    if (recurrencePattern.NoEndDate == true)
                        hasEndDate = false;
                    else hasEndDate = true;

                    //determine the recurrence type of the item
                    switch (recurTypeId)
                    {
                        case 0:
                            recurrenceType = RecurrenceType.Daily;
                            break;
                        case 1:
                            recurrenceType = RecurrenceType.Weekly;
                            break;
                        case 2:
                            recurrenceType = RecurrenceType.Monthly;
                            break;
                        case 3:
                            recurrenceType = RecurrenceType.MonthNth;
                            break;
                        case 4:
                            recurrenceType = RecurrenceType.Yearly;
                            break;
                        case 6:
                            recurrenceType = RecurrenceType.YearNth;
                            break;
                        default:
                            break;
                    }

                    if (existingWorkItem == null)
                    {
                        //Create a new work item that will act as a parent for all of its recurring items
                        var workItem = new WorkItem();

                        //if recurrence pattern has end date we save it,
                        //else we assume the end date is the end of the year to avoid large data sets
                        if (hasEndDate == true)
                            workItem.EndDateTime = recurrencePattern.PatternEndDate;
                        else workItem.EndDateTime = new DateTime(DateTime.Now.Year, 12, 31);

                        workItem.Subject = item.Parent.Subject;
                        workItem.Location = item.Parent.Location;
                        workItem.Body = item.Parent.Body;
                        workItem.OutlookEntryId = item.Parent.EntryID;
                        workItem.StartDateTime = recurrencePattern.PatternStartDate;
                        workItem.Duration = item.Parent.Duration / 60;
                        workItem.WorkItemType = WorkItemType.Appointment;
                        workItem.isRecurring = true;

                        //add the recurrence pattern
                        workItem.RecurrencePattern = new WIRecurrencePattern
                        {
                            Interval = recurrencePattern.Interval,
                            DayOfWeekMask = (DayOfWeekMask)Enum.ToObject(typeof(DayOfWeekMask), recurrencePattern.DayOfWeekMask),
                            DayOfMonth = recurrencePattern.DayOfMonth,
                            MonthOfYear = (MonthOfYear)Enum.ToObject(typeof(MonthOfYear), recurrencePattern.MonthOfYear),
                            Instance = (Instance)Enum.ToObject(typeof(Instance), recurrencePattern.Instance)
                        };

                        //add the recurring item
                        workItem.RecurringItems = new List<RecurringItem>();
                        workItem.RecurringItems.Add(new RecurringItem
                        {
                            OriginalDate = item.Start,
                            Start = item.Start,
                            End = item.End,
                            Subject = item.Subject,
                            Body = item.Body,
                            Location = item.Location,
                            UpdatedAt = DateTime.Now
                        });

                        workItem.RecurrenceType = recurrenceType;
                        workItem.CreatedByUserId = this.CurrentUser.UserId;
                        workItem.UpdatedByUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
                        this.dataContext.SaveChanges();
                    }

                    else
                    {
                        //Check if recurrence pattern has not changed
                        var existingPattern = existingWorkItem.RecurrencePattern;
                        int mismatch = 0;
                        if (existingPattern.Interval != recurrencePattern.Interval) mismatch = 1;
                        if ((int)existingPattern.DayOfWeekMask != (int)recurrencePattern.DayOfWeekMask) mismatch = 1;
                        if ((int)existingPattern.Instance != recurrencePattern.Instance) mismatch = 1;
                        if (existingPattern.DayOfMonth != recurrencePattern.DayOfMonth) mismatch = 1;
                        if ((int)existingPattern.MonthOfYear != recurrencePattern.MonthOfYear) mismatch = 1;

                        if (mismatch == 1)
                        {
                            //if the pattern has changed delete all of the old recurring items, save the new pattern and asociate it with the work item
                            foreach (var recurringItem in existingWorkItem.RecurringItems.ToList())
                            {
                                this.dataContext.RecurringItems.Remove(recurringItem);
                            }

                            this.dataContext.WIRecurrencePatterns.Remove(existingPattern);
                            existingWorkItem.RecurrencePattern = new WIRecurrencePattern
                            {
                                Interval = recurrencePattern.Interval,
                                DayOfWeekMask = (DayOfWeekMask)Enum.ToObject(typeof(DayOfWeekMask), recurrencePattern.MonthOfYear),
                                DayOfMonth = recurrencePattern.DayOfMonth,
                                MonthOfYear = (MonthOfYear)Enum.ToObject(typeof(MonthOfYear), recurrencePattern.MonthOfYear),
                                Instance = (Instance)Enum.ToObject(typeof(Instance), recurrencePattern.MonthOfYear)
                            };
                        }
                        else
                        {
                            //if pattern hasn`t changed maybe the time span of the pattern has changed, if so, update the datetime values and remove unnecessary recurring items
                            if (recurrencePattern.PatternStartDate != existingWorkItem.StartDateTime || recurrencePattern.PatternEndDate != existingWorkItem.EndDateTime)
                            {
                                foreach (var recurringItem in existingWorkItem.RecurringItems
                                    .Where(o => o.Start < recurrencePattern.PatternStartDate
                                        || o.End > recurrencePattern.PatternEndDate)
                                    .ToList())
                                {
                                    this.dataContext.RecurringItems.Remove(recurringItem);
                                }

                                existingWorkItem.StartDateTime = recurrencePattern.PatternStartDate;
                                existingWorkItem.StartDateTime = recurrencePattern.PatternEndDate;
                            }
                        }

                        var exception = exceptions.Find(o => o.AppointmentItem.Start == item.Start);

                        if (exception != null)
                        {
                            var existingRecurringItem = this.dataContext.RecurringItems.Where(o => o.OriginalDate == exception.OriginalDate).FirstOrDefault();
                            if (existingRecurringItem != null)
                            {
                                UpdateRecurringItem(existingRecurringItem, item);
                            }
                            else
                            {
                                existingWorkItem.RecurringItems.Add(new RecurringItem
                                {
                                    OriginalDate = item.Start,
                                    Exception = true,
                                    Start = item.Start,
                                    End = item.End,
                                    Subject = item.Subject,
                                    Body = item.Body,
                                    Location = item.Location,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                        }

                        else
                        {
                            var existingRecurringItem = existingWorkItem.RecurringItems.Where(o => o.OriginalDate == item.Start).FirstOrDefault();
                            if (existingRecurringItem == null)
                            {
                                existingWorkItem.RecurringItems.Add(new RecurringItem
                                {
                                    OriginalDate = item.Start,
                                    Exception = false,
                                    Start = item.Start,
                                    End = item.End,
                                    Subject = item.Subject,
                                    Body = item.Body,
                                    Location = item.Location,
                                    UpdatedAt = DateTime.Now
                                });
                            }
                            else
                            {
                                UpdateRecurringItem(existingRecurringItem, item);
                            }
                        }
                        this.dataContext.SaveChanges();
                    }
                }
            }

            //Log off
            mapiNamespace.Logoff();

            var result = new ServiceResult<OutlookItemImportServiceResult>();
            result.Messages = messages.ToArray();

            if (messages.Any(m => m.Severity == MessageSeverity.Error))
                result.Data = OutlookItemImportServiceResult.Error;
            else if (messages.Any(m => m.Severity == MessageSeverity.Warning))
                result.Data = OutlookItemImportServiceResult.OkWithWarnings;
            else
                result.Data = OutlookItemImportServiceResult.Ok;

            return result;
        }

        public WorkItem CreateNonRecurringWorkItem(Outlook.AppointmentItem item)
        {
            var workItem = new WorkItem();
            workItem.Subject = item.Subject;
            workItem.Location = item.Location;
            workItem.Body = item.Body;
            workItem.OutlookEntryId = item.EntryID;
            workItem.StartDateTime = item.Start;
            workItem.EndDateTime = item.End;
            workItem.AllDayEvent = item.AllDayEvent;
            workItem.Duration = item.Duration / 60;
            workItem.WorkItemType = WorkItemType.Appointment;
            workItem.Reminder = Reminder.Driving;
            workItem.Origin = this.CurrentUser.BaseLocation;
            workItem.isRecurring = false;
            workItem.CreatedByUserId = this.CurrentUser.UserId;
            workItem.UpdatedByUserId = this.CurrentUser.UserId;
            return workItem;
        }

        public void UpdateNonRecurringWorkItem(WorkItem existingWorkItem, Outlook.AppointmentItem item)
        {
            existingWorkItem.Subject = item.Subject;
            existingWorkItem.Location = item.Location;
            existingWorkItem.Body = item.Body;
            existingWorkItem.StartDateTime = item.Start;
            existingWorkItem.EndDateTime = item.End;
            existingWorkItem.AllDayEvent = item.AllDayEvent;
            existingWorkItem.Duration = item.Duration / 60;
            existingWorkItem.UpdatedAt = DateTime.Now;
            existingWorkItem.UpdatedByUserId = this.CurrentUser.UserId;
        }

        public void UpdateRecurringItem(RecurringItem existingRecurringItem, Outlook.AppointmentItem item)
        {
            existingRecurringItem.Start = item.Start;
            existingRecurringItem.End = item.End;
            existingRecurringItem.Subject = item.Subject;
            existingRecurringItem.Body = item.Body;
            existingRecurringItem.Location = item.Location;
            existingRecurringItem.UpdatedAt = DateTime.Now;
        }

        public void SetReminderData(ReminderScheduler scheduler, WorkItem workItem)
        {
            scheduler.Id = workItem.Id;
            scheduler.WorkItemType = workItem.WorkItemType;
            scheduler.Reminder = workItem.Reminder;
            scheduler.StartTime = workItem.StartDateTime;
            scheduler.EndTime = workItem.EndDateTime;
            scheduler.Origin = workItem.Origin;
            scheduler.Location = workItem.Location;
            scheduler.Subject = workItem.Subject;
            scheduler.MailTo = workItem.CreatedBy.Email;
            //scheduler.Url = Url.Action("Details", "WorkItem", new { id = workItem.Id }, Request.Url.Scheme);
        }
    }
}