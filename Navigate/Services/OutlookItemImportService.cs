using Microsoft.Office.Interop.Outlook;
using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public void Synchronize()
        {
            Application outlookApp = null;
            NameSpace mapiNamespace = null;
            MAPIFolder CalendarFolder = null;
            Items outlookCalendarItems = null;

            //initialize Outlook API
            outlookApp = new Application();
            mapiNamespace = outlookApp.GetNamespace("MAPI");
            CalendarFolder = mapiNamespace.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

            //filter for getting only the items whose start date is equal of greater than present time
            String filter = "[Start] >= '" + DateTime.Now.ToString("MM/dd/yyyy hh:mm") + "'";

            //get the filtered Outlook items including the recurring items
            outlookCalendarItems = CalendarFolder.Items.Restrict(filter);
            //outlookCalendarItems.Sort("[Start]");
            outlookCalendarItems.IncludeRecurrences = true;

            foreach (AppointmentItem item in outlookCalendarItems)
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

                        this.dataContext.WorkItems.Add(workItem);
                        this.dataContext.SaveChanges();
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

                            this.dataContext.SaveChanges();
                        }
                    }
                }
                else if (item.IsRecurring)
                {
                    RecurrencePattern recurrencePattern = item.GetRecurrencePattern();
                    RecurrenceType recurTypeId = 0;
                    int recurrenceType = (int)recurrencePattern.RecurrenceType;
                    bool hasEndDate;
                    if (recurrencePattern.NoEndDate == true)
                        hasEndDate = false;
                    else hasEndDate = true;

                    switch (recurrenceType)
                    {
                        case 0:
                            recurTypeId = RecurrenceType.Daily;
                            break;
                        case 1:
                            recurTypeId = RecurrenceType.Weekly;
                            break;
                        case 2:
                            recurTypeId = RecurrenceType.Monthly;
                            break;
                        case 3:
                            recurTypeId = RecurrenceType.MonthNth;
                            break;
                        case 4:
                            recurTypeId = RecurrenceType.Yearly;
                            break;
                        case 6:
                            recurTypeId = RecurrenceType.YearNth;
                            break;
                        default:
                            break;
                    }

                    if (existingWorkItem == null)
                    {
                        var pattern = new WIRecurrencePattern();
                        pattern.Interval = recurrencePattern.Interval;
                        pattern.DayOfWeekMask = (int)recurrencePattern.DayOfWeekMask;
                        pattern.DayOfMonth = recurrencePattern.DayOfMonth;
                        pattern.MonthOfYear = recurrencePattern.MonthOfYear;
                        pattern.Instance = recurrencePattern.Instance;

                        this.dataContext.WIRecurrencePatterns.Add(pattern);
                        this.dataContext.SaveChanges();

                        var workItem = new WorkItem();

                        //if recurrence pattern has end date we save it,
                        //else we only create recurring items until the end of the current year
                        if (hasEndDate == true)
                            workItem.EndDateTime = recurrencePattern.PatternEndDate;
                        else workItem.EndDateTime = new DateTime(DateTime.Now.Year, 12, 31);

                        workItem.Subject = item.Subject;
                        workItem.Location = item.Location;
                        workItem.OutlookEntryId = item.EntryID;
                        workItem.StartDateTime = recurrencePattern.PatternStartDate; 
                        workItem.EstimatedTime = item.Duration;
                        workItem.Body = item.Body;
                        workItem.WorkItemTypeId = this.dataContext.WorkItemTypes.Where(o => o.Type == "Meeting").FirstOrDefault().Id;
                        workItem.isRecurring = true;
                        workItem.WIRecurrencePatternId = pattern.Id;
                        workItem.RecurrenceType = recurTypeId;
                        workItem.CreatedByUserId = this.CurrentUser.UserId;
                        workItem.UpdatedByUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
                        this.dataContext.SaveChanges();

                        DateTime first = new DateTime(item.Start.Year,
                                            item.Start.Month,
                                            item.Start.Day,
                                            item.Start.Hour,
                                            item.Start.Minute,
                                            item.Start.Second);
                        DateTime last = workItem.EndDateTime.AddHours(item.Start.Hour).AddMinutes(item.Start.Minute).AddSeconds(item.Start.Second);
                        AppointmentItem recurringOutlookItem = null;

                        for (DateTime cur = first; cur <= last; cur = cur.AddDays(1))
                        {
                            try
                            {
                                recurringOutlookItem = recurrencePattern.GetOccurrence(cur);
                                if (recurringOutlookItem != null)
                                {
                                    var recurringItem = new RecurringItem();
                                    recurringItem.WorkItemId = workItem.Id;
                                    recurringItem.Start = recurringOutlookItem.Start;
                                    recurringItem.End = recurringOutlookItem.End;
                                    recurringItem.IndividualBody = recurringOutlookItem.Body;
                                    recurringItem.IndividualLocation = recurringOutlookItem.Location;

                                    this.dataContext.RecurringItems.Add(recurringItem);
                                    this.dataContext.SaveChanges();
                                }
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }
    }
}