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

        public ActionResult Index(string searchTerm = null)
        {
            var currentUserId = this.CurrentUser.UserId;

            var workItems = this.dataContext.WorkItems
                .Where((r => searchTerm == null || r.Subject.StartsWith(searchTerm)))
                .Select(r => new WorkItemListViewModel
                    {
                        Id = r.Id,
                        Subject = r.Subject,
                        Location = r.Location,
                        StartDateTime = r.StartDateTime,
                        EndDateTime = r.EndDateTime,
                        isCompleted = r.isCompleted
                    }
                );

            if (Request.IsAjaxRequest())
            {
                return PartialView("_WorkItems", workItems);
            }

            ViewBag.PageTitle = "Work Items";
            return View(workItems);
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
            var model = new WorkItemDataInputModel();
            populateDropDownLists(model);
            return View(model);
        }

        //
        // POST: /WorkItem/Create

        [HttpPost]
        public ActionResult Create(WorkItemDataInputModel model)
        {
            populateDropDownLists(model);
            if (ModelState.IsValid)
            {
                var workItem = model.TransformToWorkItem();
                workItem.CreatedByUserId = this.CurrentUser.UserId;
                workItem.UpdatedByUserId = this.CurrentUser.UserId;
                workItem.Categories = new List<Navigate.Models.Classifiers.Category>();

                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    var category = this.dataContext.Categories.Find(categoryId);
                    workItem.Categories.Add(category);
                }

                if (workItem.isRecurring == true)
                {
                    workItem.RecurrencePattern = model.TransformToRecurrencePattern();
                    workItem.RecurrenceType = model.RecurrenceType;

                    var occurrenceDates = GetOccurrenceDates(workItem);
                    workItem.RecurringItems = new List<RecurringItem>();
                    foreach (var date in occurrenceDates)
                    {
                        workItem.RecurringItems.Add(new RecurringItem
                        {
                            Start = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemStart.Value.Hour, model.RecurringItemStart.Value.Minute, model.RecurringItemStart.Value.Second),
                            End = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemEnd.Value.Hour, model.RecurringItemEnd.Value.Minute, model.RecurringItemEnd.Value.Second),
                            OriginalDate = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemStart.Value.Hour, model.RecurringItemStart.Value.Minute, model.RecurringItemStart.Value.Second),
                            IndividualBody = workItem.Body,
                            IndividualLocation = workItem.Location
                        });
                    }
                }

                this.dataContext.WorkItems.Add(workItem);
                this.dataContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        //
        // GET: /WorkItem/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null)
            {
                return HttpNotFound();
            }

            var model = new WorkItemDataInputModel(workItem);
            populateDropDownLists(model);
            return View(model);
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

        public ActionResult Complete(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null)
            {
                return HttpNotFound();
            }
            else
            {
                if (workItem.isCompleted == false)
                {
                    workItem.isCompleted = true;
                }
                else
                {
                    workItem.isCompleted = false;
                }
            }
            this.dataContext.SaveChanges();
            return RedirectToAction("Index");

        }

        [HttpPost]
        public JsonResult GetOutlookCalendarItems(OutlookSettingsInputModel model)
        {
            var importService = new OutlookItemImportService(this.dataContext, this.CurrentUser)
            {
                IntervalStart = model.IntervalStart,
                IntervalEnd = model.IntervalEnd
            };
            var message = "";
            var result = importService.ImportOutlookCalendarItems();
            if (result.Data == OutlookItemImportServiceResult.Ok) 
                message = "all good";

            return new JsonResult() { Data = new { Message = message } };
        }

        protected override void Dispose(bool disposing)
        {
            this.dataContext.Dispose();
            base.Dispose(disposing);
        }

        public void populateDropDownLists(WorkItemDataInputModel model)
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

            model.Categories = this.dataContext.Categories.ToList();
        }

        /// <summary>
        /// Method gets the occurrence dates of an event, given the recurrence type and pattern
        /// </summary>
        /// <param name="recurrenceType">The recurrence type</param>
        /// <param name="recurrencePattern">The recurrence pattern</param>
        /// <returns>A list of datetime objects</returns>
        private List<DateTime> GetOccurrenceDates(WorkItem workItem)
        {
            var occurrenceDates = new List<DateTime>();
            var recurrencePattern = workItem.RecurrencePattern;
            DateTime start = (DateTime)workItem.StartDateTime;
            DateTime end = workItem.EndDateTime;
            int interval = recurrencePattern.Interval;
            int dayofMonth = recurrencePattern.DayOfMonth;
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
                DateTime dayOfYear = new DateTime(start.Year, monthOfYear, dayofMonth);
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
                    NthWeekdayDayInMonth = GetNthWeekdayDateInMonth(start = start.AddMonths(1), instance, dayOfWeek);
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

        static DateTime GetNthWeekdayDateInMonth(DateTime start, int instance, List<DayOfWeek> dayOfWeek)
        {
            DateTime startOfMonth = new DateTime(start.Year, start.Month, 01);
            DateTime recurringWeekDayInMonth = GetNextWeekday(dayOfWeek[0], startOfMonth);
            DateTime NthWeekdayDayInMonth = recurringWeekDayInMonth.AddDays((instance - 1) * 7);

            return NthWeekdayDayInMonth;
        }
    }
}