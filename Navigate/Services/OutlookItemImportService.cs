using Outlook = Microsoft.Office.Interop.Outlook;
using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace Navigate.Services
{
    public class OutlookItemImportService
    {
        private NavigateDb dataContext;

        private UserProfile CurrentUser;

        public OutlookItemImportService(NavigateDb _dataContext, UserProfile _currentUser)
        {
            this.dataContext = _dataContext;
            this.CurrentUser = _currentUser;
        }
        /// <summary>
        /// Imports the items in Outlook calendar folder of current user
        /// </summary>
        public string ImportOutlookCalendarItems()
        {
            string result = String.Empty;
            int importedItemCount = 0;
            int importedOccurences = 0;

            Outlook.Application outlookApp = null;
            Outlook.NameSpace mapiNamespace = null;
            Outlook.MAPIFolder CalendarFolder = null;
            Outlook.Items outlookCalendarItems = null;

            //initialize Outlook API and log on
            outlookApp = new Outlook.Application();
            mapiNamespace = outlookApp.GetNamespace("MAPI");
            CalendarFolder = mapiNamespace.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
            mapiNamespace.Logon(Missing.Value, Missing.Value, true, true);

            //filter for getting only the items whose start date is equal of greater than present time
            String filter = "[Start] >= '" + DateTime.Now.ToString("MM/dd/yyyy hh:mm") + "'";
     
            //get the filtered Outlook items including the recurring items and sort them to expand the recurrences automatically
            outlookCalendarItems = CalendarFolder.Items.Restrict(filter);
            outlookCalendarItems.Sort("[Start]");
            outlookCalendarItems.IncludeRecurrences = true;

            foreach (Outlook.AppointmentItem item in outlookCalendarItems)
            {
                var existingWorkItem = this.dataContext.WorkItems.Where(o => o.OutlookEntryId != null && o.OutlookEntryId == item.EntryID).FirstOrDefault();

                //if item is not recurring, create a new work item or update an exisiting one 
                if (!item.IsRecurring)
                {
                    if (existingWorkItem == null)
                    {
                        var workItem = new WorkItem();
                        workItem.Subject = item.Subject;
                        workItem.Location = item.Location;
                        workItem.OutlookEntryId = item.EntryID;
                        workItem.StartDateTime = item.Start;
                        workItem.EndDateTime = item.End;
                        workItem.EstimatedTime = item.Duration;
                        workItem.Body = item.Body;
                        workItem.WorkItemTypeId = this.dataContext.WorkItemTypes.Where(o => o.Type == "Meeting").FirstOrDefault().Id;
                        workItem.isRecurring = false;
                        workItem.CreatedByUserId = this.CurrentUser.UserId;
                        workItem.UpdatedByUserId = this.CurrentUser.UserId;
                        workItem.AssignedToUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
                        importedItemCount++;
                    }
                    else
                    {
                        if (existingWorkItem.UpdatedAt <= item.LastModificationTime)
                        {
                            existingWorkItem.Subject = item.Subject;
                            existingWorkItem.Location = item.Location;
                            existingWorkItem.StartDateTime = item.Start;
                            existingWorkItem.EndDateTime = item.End;
                            existingWorkItem.EstimatedTime = item.Duration;
                            existingWorkItem.Body = item.Body;
                            existingWorkItem.UpdatedAt = DateTime.Now;
                            existingWorkItem.UpdatedByUserId = this.CurrentUser.UserId;
                            importedItemCount++;
                        }
                    }
                    this.dataContext.SaveChanges();
                }

                else if (item.IsRecurring)
                {
                    Outlook.RecurrencePattern recurrencePattern = item.GetRecurrencePattern();
                    RecurrenceType recurrenceType = 0;
                    //Get all the exceptions in recurring items, e.g. items which belong to a series of occurrences, but whose properties have been changed
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
                        //Create a new work item that can be accessed as a parent from all its recurring items
                        var workItem = new WorkItem();
                        //if recurrence pattern has end date we save it,
                        //else we only create recurring items until the end of the current year to avoid large data sets
                        if (hasEndDate == true)
                            workItem.EndDateTime = recurrencePattern.PatternEndDate;
                        else workItem.EndDateTime = new DateTime(DateTime.Now.Year, 12, 31);

                        workItem.Subject = item.Parent.Subject;
                        workItem.Location = item.Parent.Location;
                        workItem.OutlookEntryId = item.Parent.EntryID;
                        workItem.StartDateTime = recurrencePattern.PatternStartDate;
                        workItem.EstimatedTime = item.Parent.Duration;
                        workItem.Body = item.Parent.Body;
                        workItem.WorkItemTypeId = this.dataContext.WorkItemTypes.Where(o => o.Type == "Meeting").FirstOrDefault().Id;
                        workItem.isRecurring = true;
                        workItem.RecurrencePattern = new WIRecurrencePattern {
                            Interval = recurrencePattern.Interval,
                            DayOfWeekMask = (DayOfWeekMask)Enum.ToObject(typeof(DayOfWeekMask), recurrencePattern.MonthOfYear),
                            DayOfMonth = recurrencePattern.DayOfMonth,
                            MonthOfYear = (MonthOfYear)Enum.ToObject(typeof(MonthOfYear), recurrencePattern.MonthOfYear),
                            Instance = (Instance)Enum.ToObject(typeof(Instance), recurrencePattern.MonthOfYear)
                        };
                        workItem.RecurringItems.Add(new RecurringItem {
                            OriginalDate = item.Start,
                            Start = item.Start,
                            End = item.End,
                            IndividualBody = item.Body,
                            IndividualLocation = item.Location,
                        });
                        workItem.RecurrenceType = recurrenceType;
                        workItem.CreatedByUserId = this.CurrentUser.UserId;
                        workItem.UpdatedByUserId = this.CurrentUser.UserId;
                        workItem.AssignedToUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
                        this.dataContext.SaveChanges();
                        importedItemCount++;
                        importedOccurences++;
                    }

                    else
                    {
                        //Check if recurrence pattern has not changed
                        var existingPattern = this.dataContext.WIRecurrencePatterns.Where(o => o.WorkItemId == existingWorkItem.Id).FirstOrDefault();
                        int mismatch = 0;
                        if (existingPattern.Interval != recurrencePattern.Interval) mismatch = 1;
                        if ((int)existingPattern.DayOfWeekMask != (int)recurrencePattern.DayOfWeekMask) mismatch = 1;
                        if ((int)existingPattern.Instance != recurrencePattern.Instance) mismatch = 1;
                        if (existingPattern.DayOfMonth != recurrencePattern.DayOfMonth) mismatch = 1;
                        if ((int)existingPattern.MonthOfYear != recurrencePattern.MonthOfYear) mismatch = 1;

                        if (mismatch == 1)
                        {
                            //if the pattern has changed delete all of the old recurring items, save the new pattern and asociate it with the work item
                            var oldRecurringItems = this.dataContext.RecurringItems.Where(o => o.WorkItemId == existingWorkItem.Id).ToList();
                            foreach (var recurringItem in oldRecurringItems)
                            {
                                this.dataContext.RecurringItems.Remove(recurringItem);
                            }
                            var pattern = new WIRecurrencePattern();
                            pattern.Interval = recurrencePattern.Interval;
                            pattern.DayOfWeekMask = (DayOfWeekMask)Enum.ToObject(typeof(DayOfWeekMask), recurrencePattern.MonthOfYear);
                            pattern.DayOfMonth = recurrencePattern.DayOfMonth;
                            pattern.MonthOfYear = (MonthOfYear)Enum.ToObject(typeof(MonthOfYear), recurrencePattern.MonthOfYear);
                            pattern.Instance = (Instance)Enum.ToObject(typeof(Instance), recurrencePattern.MonthOfYear);
                            this.dataContext.WIRecurrencePatterns.Add(pattern);
                            this.dataContext.SaveChanges();

                            //existingWorkItem.WIRecurrencePatternId = pattern.WorkItemId;
                            this.dataContext.SaveChanges();
                        }

                        var exception = exceptions.Find(o => o.AppointmentItem.Start == item.Start);

                        if (exception != null)
                        {          
                            var existingRecurringItem = this.dataContext.RecurringItems.Where(o => o.OriginalDate == exception.OriginalDate).FirstOrDefault();
                            if (existingRecurringItem != null)
                            {
                                existingRecurringItem.Start = item.Start;
                                existingRecurringItem.End = item.End;
                                existingRecurringItem.IndividualBody = item.Body;
                                existingRecurringItem.IndividualLocation = item.Location;
                                this.dataContext.SaveChanges();
                            }
                        }

                        else
                        {
                            var recurringItem = new RecurringItem();
                            recurringItem.OriginalDate = item.Start;
                            recurringItem.WorkItemId = this.dataContext.WorkItems.Where(o => o.OutlookEntryId == item.EntryID).FirstOrDefault().Id;
                            recurringItem.Start = item.Start;
                            recurringItem.End = item.End;
                            recurringItem.IndividualBody = item.Body;
                            recurringItem.IndividualLocation = item.Location;
                            this.dataContext.RecurringItems.Add(recurringItem);
                            this.dataContext.SaveChanges();
                        }
                    }
                }
            }
            //Log off
            mapiNamespace.Logoff();
            result = String.Format("A total of {0] items were successfully imported, of which {1} were occurrences of recurring items", importedItemCount, importedOccurences);

            return result;
        }
    }
}
