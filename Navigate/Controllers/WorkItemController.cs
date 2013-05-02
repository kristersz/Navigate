using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Navigate.Models;
using Navigate.Models.Classifiers;
using WebMatrix.WebData;
using Navigate.ViewModels;
using Microsoft.Office.Interop.Outlook;
using Navigate.Services;
using System.ComponentModel.DataAnnotations;

namespace Navigate.Controllers
{   
    [Authorize]
    public class WorkItemController : BaseController
    {
        //
        // GET: /WorkItem/

        public ActionResult Index()
        {
            var currentUserId = this.CurrentUser.UserId;

            var workItems = this.dataContext.WorkItems
                .Where(r => r.AssignedToUserId == currentUserId);

            ViewBag.PageTitle = "Work Items";
            return View(workItems.ToList());
        }

        //
        // GET: /WorkItem/Details/5

        public ActionResult Details(int id = 0)
        {
            WorkItem workitem = this.dataContext.WorkItems.Find(id);
            if (workitem == null)
            {
                return HttpNotFound();
            }
            return View(workitem);
        }

        //
        // GET: /WorkItem/Create

        public ActionResult Create()
        {
            var model = new WorkItemCreateViewModel();
            populateDropDownLists(model);
            return View(model);
        }

        //
        // POST: /WorkItem/Create

        [HttpPost]
        public ActionResult Create(WorkItemCreateViewModel model)
        {
            populateDropDownLists(model);
            if (ModelState.IsValid)
            {
                var workItem = model.TransformToWorkItem();
                workItem.CreatedByUserId = this.CurrentUser.UserId;
                workItem.UpdatedByUserId = this.CurrentUser.UserId;
                WIRecurrencePattern recurrencePattern = new WIRecurrencePattern();

                if (workItem.isRecurring == true)
                {
                    recurrencePattern = model.TransformToRecurrencePattern();

                    this.dataContext.WIRecurrencePatterns.Add(recurrencePattern);
                    this.dataContext.SaveChanges();

                    workItem.WIRecurrencePatternId = recurrencePattern.Id;
                }

                this.dataContext.WorkItems.Add(workItem);
                this.dataContext.SaveChanges();

                if (workItem.isRecurring == true)
                {
                    var occurrenceDates = GetOccurrenceDates(workItem, recurrencePattern);
                    foreach (var date in occurrenceDates)
                    {
                        var recurringItem = new RecurringItem
                        {
                            WorkItemId = workItem.Id,
                            Start = date,
                            End = workItem.EndDateTime,
                            OriginalDate = (DateTime)workItem.StartDateTime,
                            IndividualBody = workItem.Body,
                            IndividualLocation = workItem.Location
                        };
                        this.dataContext.RecurringItems.Add(recurringItem);
                        this.dataContext.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }

            return View(model);
        }

        //
        // GET: /WorkItem/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WorkItem workitem = this.dataContext.WorkItems.Find(id);
            if (workitem == null)
            {
                return HttpNotFound();
            }
            return View(workitem);
        }

        //
        // POST: /WorkItem/Edit/5

        [HttpPost]
        public ActionResult Edit(WorkItem workitem)
        {
            if (ModelState.IsValid)
            {
                this.dataContext.Entry(workitem).State = EntityState.Modified;
                workitem.UpdatedByUserId = this.CurrentUser.UserId;
                this.dataContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(workitem);
        }

        //
        // GET: /WorkItem/Delete/5

        public ActionResult Delete(int id = 0)
        {
            WorkItem workitem = this.dataContext.WorkItems.Find(id);
            if (workitem == null)
            {
                return HttpNotFound();
            }
            return View(workitem);
        }

        //
        // POST: /WorkItem/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            WorkItem workitem = this.dataContext.WorkItems.Find(id);
            this.dataContext.WorkItems.Remove(workitem);
            this.dataContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Navigate(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null)
            {
                return HttpNotFound();
            }

            ViewBag.Location = workItem.Location;

            return View();
        }

        #region ["Outlook logic"]

        [HttpPost]
        public JsonResult GetOutlookCalendarItems()
        {
            string message = "Success";
            var importService = new OutlookItemImportService(this.dataContext, this.CurrentUser);
            importService.ImportOutlookCalendarItems();

            return new JsonResult() { Data = new { Message = message } };
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            this.dataContext.Dispose();
            base.Dispose(disposing);
        }

        public void populateDropDownLists(WorkItemCreateViewModel model)
        {
            var types = (from r in this.dataContext.WorkItemTypes
                            select r).ToList();
            model.AllWorkItemTypes = types.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.Type
            });
            var users = (from r in this.dataContext.UserProfiles
                            select r).ToList();
            model.AllUsers = users.Select(o => new SelectListItem
            {
                Value = o.UserId.ToString(),
                Text = o.UserName
            });
        }

        /// <summary>
        /// Method gets the occurrence dates of an event, given the recurrence type and pattern
        /// </summary>
        /// <param name="recurrenceType">The recurrence type</param>
        /// <param name="recurrencePattern">The recurrence pattern</param>
        /// <returns>A list of datetime objects</returns>
        private List<DateTime> GetOccurrenceDates(WorkItem workItem, WIRecurrencePattern recurrencePattern)
        {
            var occurrenceDates = new List<DateTime>();
            DateTime start = (DateTime)workItem.StartDateTime;
            DateTime end = workItem.EndDateTime;
            int interval = recurrencePattern.Interval;
            int dayofMonth = recurrencePattern.DayOfMonth;
            int instance = (int)recurrencePattern.Instance;
            int monthOfyear = (int)recurrencePattern.MonthOfYear;

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
                    for (DateTime cur = next; cur <= end; cur = cur.AddDays(interval))
                    {
                        occurrenceDates.Add(cur);
                    }
                }
                return occurrenceDates;
            }
            else if (workItem.RecurrenceType == RecurrenceType.Monthly)
            {
                DateTime startOfMonth = new DateTime(start.Year, start.Month, 01);
                DateTime recurringDayInMonth = startOfMonth.AddDays(dayofMonth);
                for (DateTime cur = recurringDayInMonth; cur <= end; cur = cur.AddMonths(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else if (workItem.RecurrenceType == RecurrenceType.MonthNth)
            {
                var dayOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);
                DateTime startOfMonth = new DateTime(start.Year, start.Month, 01);
                DateTime recurringWeekDayInMonth = GetNextWeekday(dayOfWeek[0], startOfMonth);
                DateTime NthWeekdayDayInMonth = recurringWeekDayInMonth.AddDays((instance - 1) * 7);
                for (DateTime cur = NthWeekdayDayInMonth; cur <= end; cur = cur.AddMonths(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else if (workItem.RecurrenceType == RecurrenceType.Yearly)
            {
                DateTime dayOfYear = new DateTime(start.Year, monthOfyear, dayofMonth);
                for (DateTime cur = dayOfYear; cur <= end; cur = cur.AddYears(interval))
                {
                    occurrenceDates.Add(cur);
                }
                return occurrenceDates;
            }
            else
            {
                var dayOfWeek = GetRecurringDaysOfWeek((int)recurrencePattern.DayOfWeekMask);
                DateTime startOfMonth = new DateTime(start.Year, start.Month, 01);
                DateTime recurringWeekDayInMonth = GetNextWeekday(dayOfWeek[0], startOfMonth);
                DateTime NthWeekdayDayInMonth = recurringWeekDayInMonth.AddDays((instance - 1) * 7);
                for (DateTime cur = NthWeekdayDayInMonth; cur <= end; cur = cur.AddYears(interval))
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