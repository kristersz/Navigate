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

        public void ImportOutlookCalendarItems()
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
            outlookCalendarItems.Sort("[Start]");
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
                        workItem.AssignedToUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
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
                        }
                    }
                    this.dataContext.SaveChanges();
                }

                else if (item.IsRecurring)
                {
                    RecurrencePattern recurrencePattern = item.GetRecurrencePattern();
                    RecurrenceType recurrenceType = 0;
                    //Get all the exceptions in recurring items, e.g. items which belong to a series of occurrences, but whose properties have been solely changed
                    List<Microsoft.Office.Interop.Outlook.Exception> exceptions = new List<Microsoft.Office.Interop.Outlook.Exception>();
                    foreach (Microsoft.Office.Interop.Outlook.Exception exception in recurrencePattern.Exceptions)
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
                        //Save the pattern for future reference
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
                        //else we only create recurring items until the end of the current year to avoid infinite data sets
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
                        workItem.WIRecurrencePatternId = pattern.Id;
                        workItem.RecurrenceType = recurrenceType;
                        workItem.CreatedByUserId = this.CurrentUser.UserId;
                        workItem.UpdatedByUserId = this.CurrentUser.UserId;
                        workItem.AssignedToUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
                        this.dataContext.SaveChanges();

                        var recurringItem = new RecurringItem();
                        recurringItem.OriginalDate = item.Start;
                        recurringItem.WorkItemId = workItem.Id;
                        recurringItem.Start = item.Start;
                        recurringItem.End = item.End;
                        recurringItem.IndividualBody = item.Body;
                        recurringItem.IndividualLocation = item.Location;

                        this.dataContext.RecurringItems.Add(recurringItem);
                        this.dataContext.SaveChanges();
                       
                    }
                    else
                    {
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
        }

        /// <summary>
        /// Method gets the occurrence dates of an event, given the recurrence type and pattern
        /// </summary>
        /// <param name="recurrenceType">The recurrence type</param>
        /// <param name="recurrencePattern">The recurrence pattern</param>
        /// <returns>A list of datetime objects</returns>
        private List<DateTime> GetOccurrenceDates(RecurrenceType recurrenceType, RecurrencePattern recurrencePattern)
        {
            var occurrenceDates = new List<DateTime>();
            DateTime start = recurrencePattern.PatternStartDate;
            DateTime end = new DateTime();
            if (recurrencePattern.NoEndDate == false)
                end = recurrencePattern.PatternEndDate;
            else end = new DateTime(DateTime.Now.Year, 12, 31);
            int interval = recurrencePattern.Interval;
            int dayofMonth = recurrencePattern.DayOfMonth;
            int instance = recurrencePattern.Instance;
            int monthOfyear = recurrencePattern.MonthOfYear;

            if (recurrenceType == RecurrenceType.Daily)
            {
                for (DateTime cur = start; cur <= end; cur.AddDays(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else if (recurrenceType == RecurrenceType.Weekly)
            {
                var daysOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);

                foreach (DayOfWeek dayOfWeek in daysOfWeek)
                {
                    DateTime next = GetNextWeekday(dayOfWeek, start);
                    for (DateTime cur = next; cur <= end; cur.AddDays(interval))
                    {
                        occurrenceDates.Add(cur);
                    }
                }
                return occurrenceDates;
            }
            else if (recurrenceType == RecurrenceType.Monthly)
            {
                DateTime startOfMonth = new DateTime(recurrencePattern.PatternStartDate.Year, recurrencePattern.PatternStartDate.Month, 01);
                DateTime recurringDayInMonth = startOfMonth.AddDays(dayofMonth);
                for (DateTime cur = recurringDayInMonth; cur <= end; cur.AddMonths(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else if (recurrenceType == RecurrenceType.MonthNth)
            {
                var dayOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);
                DateTime startOfMonth = new DateTime(recurrencePattern.PatternStartDate.Year, recurrencePattern.PatternStartDate.Month, 01);
                DateTime recurringWeekDayInMonth = GetNextWeekday(dayOfWeek[0], startOfMonth);
                DateTime NthWeekdayDayInMonth = recurringWeekDayInMonth.AddDays((instance - 1) * 7);
                for (DateTime cur = NthWeekdayDayInMonth; cur <= end; cur.AddMonths(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else if (recurrenceType == RecurrenceType.Yearly)
            {
                DateTime dayOfYear = new DateTime(recurrencePattern.PatternStartDate.Year, monthOfyear, dayofMonth);
                for (DateTime cur = dayOfYear; cur <= end; cur.AddYears(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else
            {
                var dayOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);
                DateTime startOfMonth = new DateTime(recurrencePattern.PatternStartDate.Year, recurrencePattern.PatternStartDate.Month, 01);
                DateTime recurringWeekDayInMonth = GetNextWeekday(dayOfWeek[0], startOfMonth);
                DateTime NthWeekdayDayInMonth = recurringWeekDayInMonth.AddDays((instance - 1) * 7);
                for (DateTime cur = NthWeekdayDayInMonth; cur <= end; cur.AddYears(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
        }

        /// <summary>
        /// Method for converting the DaysOfWeekMask into days of week representing the recurring days in a week
        /// </summary>
        /// <param name="mask">The days of week mask</param>
        /// <returns>A list of DayOfWeek objects</returns>
        private List<DayOfWeek> GetRecurringDaysOfWeek(int mask)
        {
            var days = new List<DayOfWeek>();

            while (mask != 0)
            {
                if (mask >= 64)
                {
                    mask = mask % 64;
                    days.Add(DayOfWeek.Saturday);
                }
                else if (mask >= 32)
                {
                    mask = mask % 32;
                    days.Add(DayOfWeek.Friday);
                }
                else if (mask >= 16)
                {
                    mask = mask % 16;
                    days.Add(DayOfWeek.Thursday);
                }
                else if (mask >= 8)
                {
                    mask = mask % 8;
                    days.Add(DayOfWeek.Wednesday);
                }
                else if (mask >= 4)
                {
                    mask = mask % 4;
                    days.Add(DayOfWeek.Tuesday);
                }
                else if (mask >= 2)
                {
                    mask = mask % 2;
                    days.Add(DayOfWeek.Monday);
                }
                else if (mask >= 1)
                {
                    mask = mask % 1;
                    days.Add(DayOfWeek.Sunday);
                }
            }
            return days;
        }

        /// <summary>
        /// Method for getting the next specified weekday after the pattern start date
        /// </summary>
        /// <param name="day">The day of week whose date is to be determined</param>
        /// <param name="start">The pattern start date</param>
        /// <returns>A datetime object of the next weekday</returns>
        static DateTime GetNextWeekday(DayOfWeek day, DateTime start)
        {
            DateTime result = start;
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }
    }
}
