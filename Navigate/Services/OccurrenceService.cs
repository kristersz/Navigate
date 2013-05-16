using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.Services
{
    public class OccurrenceService
    {
        /// <summary>
        /// Method gets the occurrence dates of an event, given the recurrence type and pattern
        /// </summary>
        /// <param name="recurrenceType">The recurrence type</param>
        /// <param name="recurrencePattern">The recurrence pattern</param>
        /// <returns>A list of datetime objects</returns>
        public List<DateTime> GetOccurrenceDates(WorkItem workItem)
        {
            var occurrenceDates = new List<DateTime>();
            var recurrencePattern = workItem.RecurrencePattern;
            DateTime start = (DateTime)workItem.StartDateTime;
            DateTime end = workItem.EndDateTime;
            int interval = recurrencePattern.Interval;
            int dayOfMonth = recurrencePattern.DayOfMonth;
            int instance = (int)recurrencePattern.Instance;
            int monthOfYear = (int)recurrencePattern.MonthOfYear;

            if (workItem.RecurrenceType == RecurrenceType.Daily)
            {
                for (DateTime cur = start; cur <= end; cur = cur.AddDays(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }

            else if (workItem.RecurrenceType == RecurrenceType.Weekly)
            {
                var daysOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);

                foreach (DayOfWeek dayOfWeek in daysOfWeek)
                {
                    DateTime next = GetNextWeekday(dayOfWeek, start);
                    for (DateTime cur = next; cur <= end; cur = cur.AddDays(interval * 7))
                    {
                        occurrenceDates.Add(cur);
                    }
                }
                return occurrenceDates;
            }

            else if (workItem.RecurrenceType == RecurrenceType.Monthly)
            {
                DateTime startOfMonth = new DateTime(start.Year, start.Month, 01);
                DateTime recurringDayInMonth = startOfMonth.AddDays(dayOfMonth - 1);
                for (DateTime cur = recurringDayInMonth; cur <= end; cur = cur.AddMonths(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }

            else if (workItem.RecurrenceType == RecurrenceType.MonthNth)
            {
                var dayOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);
                DateTime NthWeekdayDayInMonth = GetNthWeekdayDateInMonth(start, instance, dayOfWeek);
                if (NthWeekdayDayInMonth < start)
                {
                    NthWeekdayDayInMonth = GetNthWeekdayDateInMonth(start = start.AddMonths(1), instance, dayOfWeek);
                }
                for (DateTime cur = NthWeekdayDayInMonth; cur <= end; cur = GetNthWeekdayDateInMonth(cur = cur.AddMonths(interval), instance, dayOfWeek))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }

            else if (workItem.RecurrenceType == RecurrenceType.Yearly)
            {
                DateTime dayOfYear = new DateTime(start.Year, monthOfYear, dayOfMonth);
                for (DateTime cur = dayOfYear; cur <= end; cur = cur.AddYears(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }

            else
            {
                var dayOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);
                DateTime NthWeekdayDayInMonth = GetNthWeekdayDateInMonth(start, instance, dayOfWeek);
                if (NthWeekdayDayInMonth < start)
                {
                    NthWeekdayDayInMonth = GetNthWeekdayDateInMonth(start = start.AddYears(1), instance, dayOfWeek);
                }
                for (DateTime cur = NthWeekdayDayInMonth; cur <= end; cur = GetNthWeekdayDateInMonth(cur = cur.AddYears(interval), instance, dayOfWeek))
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

        /// <summary>
        /// Method for getting the datetime of the specified day in a specified month
        /// </summary>
        /// <param name="start">Start of the interval</param>
        /// <param name="instance">The instance</param>
        /// <param name="dayOfWeek">The day of week</param>
        /// <returns>DateTime object</returns>
        static DateTime GetNthWeekdayDateInMonth(DateTime start, int instance, List<DayOfWeek> dayOfWeek)
        {
            DateTime startOfMonth = new DateTime(start.Year, start.Month, 01);
            DateTime recurringWeekDayInMonth = GetNextWeekday(dayOfWeek[0], startOfMonth);
            DateTime NthWeekdayDayInMonth = recurringWeekDayInMonth.AddDays((instance - 1) * 7);

            return NthWeekdayDayInMonth;
        }
    }
}