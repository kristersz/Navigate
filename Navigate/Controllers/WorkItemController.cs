using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Navigate.Models;
using Navigate.Models.Classifiers;
using Navigate.ViewModels;
using Navigate.Services;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using Navigate.Quartz;
using System.Globalization;
using UnconstrainedMelody;
using PagedList;

namespace Navigate.Controllers
{   
    [Authorize]
    public class WorkItemController : BaseController
    {
        /// <summary>
        /// Displays the list of work items, provides filtering, searching and sorting of returned elements
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="filter">The filter</param>
        /// <param name="sortOrder">The sort order</param>
        /// <param name="category">The chosen category</param>
        /// <param name="page">The current page</param>
        /// <returns>The index view</returns>
        public ActionResult Index(string searchTerm = null, string filter  = null, string sortOrder = null, string category = null, int page = 1)
        {
            var currentUserId = this.CurrentUser.UserId;

            var workItems = this.dataContext.WorkItems
                .Where((r => r.CreatedByUserId == this.CurrentUser.UserId))
                .Select(r => new WorkItemListViewModel
                    {
                        Id = r.Id,
                        Subject = r.Subject,
                        Location = r.Location,
                        StartDateTime = r.StartDateTime,
                        EndDateTime = r.EndDateTime,
                        isCompleted = r.isCompleted,
                        isRecurring = r.isRecurring,
                        Priority = r.Priority,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Categories = r.Categories,
                        RecurringItems = r.RecurringItems,
                        NextRecurringItem = r.RecurringItems.OrderBy(o => o.Start).FirstOrDefault(o => o.isCompleted == false),
                    }
                );

            IEnumerable<SelectListItem> categoriesList = new List<SelectListItem>();
            categoriesList = this.dataContext.Categories
                .Where(o => o.CreatedByUserId == this.CurrentUser.UserId)
                .ToList()
                .Select(o => new SelectListItem
                    {
                        Value = o.Id.ToString(),
                        Text = o.Name
                    });


            ViewBag.Category = categoriesList;

            if (!String.IsNullOrEmpty(searchTerm))
            {
                workItems = workItems.Where(r => r.Subject.Contains(searchTerm));
            }

            if (!String.IsNullOrEmpty(category))
            {
                var catId = Convert.ToInt32(category);
                workItems = workItems.Where(r => r.Categories.Any(o => o.Id == catId));
            }

            switch (filter)
            {
                case "all":
                    break;
                case "completed":
                    workItems = workItems.Where(r => r.isCompleted == true);
                    break;
                case "starred":
                    workItems = workItems.Where(r => r.Priority != (int)WorkItemPriority.None);
                    break;
                case "late":
                    workItems = workItems.Where(r => r.EndDateTime < DateTime.Today && r.isCompleted == false);
                    break;
                case "today":
                    workItems = workItems.Where(r => EntityFunctions.TruncateTime(r.EndDateTime) == EntityFunctions.TruncateTime(DateTime.Now) && r.EndDateTime >= DateTime.Now);
                    break;
                default:
                    break;
            }

            switch (sortOrder)
            {
                case "deadline":
                    workItems = workItems.OrderBy(r => r.EndDateTime);
                    break;
                case "priority":
                    workItems = workItems.OrderByDescending(r => r.Priority);
                    break;
                case "changedate":
                    workItems = workItems.OrderByDescending(r => r.UpdatedAt);
                    break;
                case "createdate":
                    workItems = workItems.OrderByDescending(r => r.CreatedAt);
                    break;
                case "title":
                    workItems = workItems.OrderBy(r => r.Subject);
                    break;
                default:
                    workItems = workItems.OrderBy(r => r.EndDateTime);
                    break;
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_WorkItems", workItems);
            }

            return View(workItems);
        }

        /// <summary>
        /// Displays the detailed view of a given work item
        /// </summary>
        /// <param name="id">The work item id</param>
        /// <returns>The details view</returns>
        public ActionResult Details(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null)
            {
                return HttpNotFound();
            }
            var workItems = this.dataContext.WorkItems
                .Where((r => r.CreatedByUserId == this.CurrentUser.UserId && r.Id == id))
                .Select(r => new WorkItemDetailsViewModel
                {
                    Id = r.Id,
                    Subject = r.Subject,
                    Location = r.Location,
                    StartDateTime = r.StartDateTime,
                    EndDateTime = r.EndDateTime,
                    Duration = r.Duration.Value,
                    Body = r.Body,
                    WorkItemType = r.WorkItemType,
                    isCompleted = r.isCompleted,
                    isRecurring = r.isRecurring,
                    Priority = r.Priority,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Categories = r.Categories,
                    RecurringItems = r.RecurringItems,
                }
            ).Single();

            ViewBag.Title = "Apskatīt";
            ViewBag.Pagetitle = "Apskatīti uzdevumu " + workItem.Subject;
            return View(workItems);
        }

        /// <summary>
        /// Displays the work item create form
        /// </summary>
        /// Parameters are used only when the user selects a time span in the calendar and invokes this method
        /// <param name="start">Start datetime of a work item</param>
        /// <param name="end">End datetime of a work item</param>
        /// <returns>The create view</returns>
        [HttpGet]
        public ActionResult Create(string start = null, string end = null)
        {
            var model = new WorkItemDataInputModel();
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end))
            {
                DateTime dtStart = DateTime.ParseExact(start, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                DateTime dtEnd = DateTime.ParseExact(end, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                model.StartDate = dtStart;
                model.EndDate = dtEnd;
                model.WorkItemType = WorkItemType.Appointment;
            }

            PopulateDropDownLists(model);
            model.Origin = this.CurrentUser.BaseLocation;
            ViewBag.Title = "Izveidot";
            ViewBag.Pagetitle = "Izveidot uzdevumu";
            return View(model);
        }

        /// <summary>
        /// The method that responds to the HttpPost request of create action
        /// </summary>
        /// <param name="model">The WorkItemDataInputModel</param>
        /// <returns>Redirect to index action if there are no errors in the create form, otherwise redisplays the create form with the error messages</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(WorkItemDataInputModel model)
        {
            PopulateDropDownLists(model);

            if (ModelState.IsValid)
            {
                if (model.WorkItemType == WorkItemType.None)
                {
                    ModelState.AddModelError("", "Uzdevuma tips ir obligāts lauks");
                    return View(model);
                }

                if ((model.Reminder == Reminder.Driving || model.Reminder == Reminder.Walking) && model.Location == null)
                {
                    ModelState.AddModelError("", "Lai izvēlētos šo atgādinājumu, ir jānorāda uzdevuma atrašanās vietas adrese");
                    return View(model);
                }

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
                    CreateOccurrences(workItem, model);
                }

                this.dataContext.WorkItems.Add(workItem);
                this.dataContext.SaveChanges();

                if (workItem.Reminder != Reminder.None)
                {
                    var scheduler = new ReminderScheduler();
                    scheduler.ScheduleReminder(workItem);
                }

                TempData["Message"] = "Uzdevums " + workItem.Subject + " veiksmīgi izveidots";
                TempData["Alert-Level"] = "alert-success";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Lūdzu, izlabojiet kļūdas un atkārtoti nospiediet Izveidot");
            return View(model);
        }

        /// <summary>
        /// Displays the work item edit form
        /// </summary>
        /// <param name="id">The work item id</param>
        /// <returns>The edit view</returns>
        public ActionResult Edit(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null || workItem.CreatedByUserId != this.CurrentUser.UserId)
            {
                return HttpNotFound();
            }

            var model = new WorkItemDataInputModel(workItem);
            PopulateDropDownLists(model);
            foreach (var category in workItem.Categories)
            {
                model.SelectedCategoryIds.Add((int)category.Id);
            }

            ViewBag.Title = "Rediģēt";
            ViewBag.Pagetitle = "Rediģēt uzdevumu " + workItem.Subject;
            return View(model);
        }

        /// <summary>
        /// The method that responds to the HttpPost request of the edit action
        /// </summary>
        /// <param name="model">The WorkItemDataInputModel</param>
        /// <returns>Redirect to index action if there are no errors in the edit form, otherwise redisplays the edit form with the error messages</returns>
        [HttpPost]
        public ActionResult Edit(WorkItemDataInputModel model)
        {
            PopulateDropDownLists(model);
            if (model.WorkItemType == WorkItemType.None)
            {
                ModelState.AddModelError("", "Uzdevuma tips ir obligāts lauks");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                WorkItem workItem = this.dataContext.WorkItems.Where(o => o.Id == model.WorkItemId).FirstOrDefault();
                if (workItem != null)
                {
                    model.UpdateWorkItem(workItem);

                    if (workItem.isRecurring == true)
                    {
                        var newPattern = model.TransformToRecurrencePattern();
                        if (workItem.RecurrencePattern == null)
                        {
                            workItem.RecurrencePattern = newPattern;
                            workItem.RecurrenceType = model.RecurrenceType;
                            CreateOccurrences(workItem, model);
                        }
                        else
                        {
                            //Check if recurrence pattern has not changed
                            var existingPattern = workItem.RecurrencePattern;
                            int mismatch = 0;
                            if (existingPattern.Interval != newPattern.Interval) mismatch = 1;
                            if ((int)existingPattern.DayOfWeekMask != (int)newPattern.DayOfWeekMask) mismatch = 1;
                            if ((int)existingPattern.Instance != (int)newPattern.Instance) mismatch = 1;
                            if (existingPattern.DayOfMonth != newPattern.DayOfMonth) mismatch = 1;
                            if ((int)existingPattern.MonthOfYear != (int)newPattern.MonthOfYear) mismatch = 1;

                            if (mismatch == 1)
                            {
                                //if the pattern has changed delete all of the old recurring items, save the new pattern and asociate it with the work item
                                RemoveRecurringItems(workItem);
                                this.dataContext.WIRecurrencePatterns.Remove(existingPattern);
                                workItem.RecurrencePattern = newPattern;
                                workItem.RecurrenceType = model.RecurrenceType;
                                CreateOccurrences(workItem, model);
                            }
                            else
                            {
                                //if pattern hasn`t changed maybe the time span of the pattern has changed, if so, update the datetime values and remove unnecessary recurring items (or add them)
                                if (model.StartDate != workItem.StartDateTime || model.EndDate != workItem.EndDateTime)
                                {
                                    foreach (var recurringItem in workItem.RecurringItems
                                        .Where(o => o.Start < workItem.StartDateTime
                                            || o.End > workItem.EndDateTime)
                                        .ToList())
                                    {
                                        this.dataContext.RecurringItems.Remove(recurringItem);
                                    }

                                    workItem.StartDateTime = model.StartDate;
                                    workItem.StartDateTime = model.EndDate;
                                    CreateOccurrences(workItem, model);
                                }
                            }
                        }
                    }
                    else 
                    {
                        if (workItem.RecurrencePattern != null)
                        {
                            var existingPattern = workItem.RecurrencePattern;
                            RemoveRecurringItems(workItem);
                            this.dataContext.WIRecurrencePatterns.Remove(existingPattern);
                            workItem.RecurrenceType = null;
                        }
                    }

                    workItem.UpdatedAt = DateTime.Now;
                    workItem.UpdatedByUserId = this.CurrentUser.UserId;
                    this.dataContext.SaveChanges();
                }

                TempData["Message"] = "Uzdevums " + workItem.Subject + " veiksmīgi atjaunots";
                TempData["Alert-Level"] = "alert-success";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        /// <summary>
        /// Display the delete confirmation view
        /// </summary>
        /// <param name="id">The work item id</param>
        /// <returns>View</returns>
        public ActionResult Delete(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null || workItem.CreatedByUserId != this.CurrentUser.UserId)
            {
                return HttpNotFound();
            }

            return View(workItem);
        }

        
        /// <summary>
        /// Deletes the confirmed work item
        /// </summary>
        /// <param name="id">The work item id</param>
        /// <returns>Redirects to Index action</returns>
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem.RecurrencePattern != null)
            {
                this.dataContext.WIRecurrencePatterns.Remove(workItem.RecurrencePattern);
            }
            this.dataContext.WorkItems.Remove(workItem);
            this.dataContext.SaveChanges();

            TempData["Message"] = "Uzdevums " + workItem.Subject + " veiksmīgi izdzēsts";
            TempData["Alert-Level"] = "alert-success";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Displays the navigation form
        /// </summary>
        /// <param name="id">The work item id</param>
        /// <returns>The view</returns>
        public ActionResult Navigate(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            if (workItem == null || workItem.CreatedByUserId != this.CurrentUser.UserId)
            {
                return HttpNotFound();
            }

            ViewBag.Location = workItem.Location;

            return View();
        }

        /// <summary>
        /// Changes the completion status of a work item
        /// </summary>
        /// <param name="id">The work item id</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        public JsonResult ChangeStatus(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            var message = "";

            if (workItem == null || workItem.CreatedByUserId != this.CurrentUser.UserId)
            {
                message = "Uzdevums netika atrasts";
                return new JsonResult() { Data = new { IsValid = false, Message = message } };
            }

            if (workItem.isRecurring == true)
            {
                var recurringItem = workItem.RecurringItems.OrderBy(o => o.Start).FirstOrDefault(o => o.isCompleted == false);
                var nextRecurringItem = workItem.RecurringItems.OrderBy(o => o.Start).SkipWhile(o => o.isCompleted != false).Skip(1).FirstOrDefault();
                if (recurringItem == null)
                {
                    message = "Periodiskais uzdevums netika atrasts";
                    return new JsonResult() { Data = new { IsValid = false, Message = message } };
                }
                else
                {
                    if (recurringItem.isCompleted == false)
                    {
                        recurringItem.isCompleted = true;
                        recurringItem.CompletedAt = DateTime.Now;
                        if (nextRecurringItem == null)
                        {
                            workItem.isCompleted = true;
                            workItem.CompletedAt = DateTime.Now;
                            message = "Periodiskais uzdevums " + recurringItem.Subject.ToString() + " veiksmīgi tika atzīmēts kā izpildīts, šis bija pēdējais notikums šajā notikumu sērijā";
                        }
                        else message = "Periodiskais uzdevums " + recurringItem.Subject.ToString() + " veiksmīgi tika atzīmēts kā izpildīts, nākamais notikums: " + nextRecurringItem.Start;
                    }
                }
            }
            else
            {
                if (workItem == null)
                {
                    message = "Uzdevums netika atrasts";
                    return new JsonResult() { Data = new { IsValid = false, Message = message } };
                }
                else
                {
                    if (workItem.isCompleted == false)
                    {
                        workItem.isCompleted = true;
                        workItem.CompletedAt = DateTime.Now;
                        message = "Uzdevums " + workItem.Subject.ToString() + " veiksmīgi tika atzīmēts kā izpildīts";
                    }
                    else
                    {
                        workItem.isCompleted = false;
                        workItem.CompletedAt = null;
                        message = "Uzdevums " + workItem.Subject.ToString() + " veiksmīgi tika atzīmēts kā neizpildīts";
                    }
                }
            }
            this.dataContext.SaveChanges();
            return new JsonResult() { Data = new { IsValid = true, Message = message } };
        }

        /// <summary>
        /// Imports events from Outlook calendar within the specified interval
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        public JsonResult GetOutlookCalendarItems(OutlookSettingsInputModel model)
        {          
            if (ModelState.IsValid)
            {
                string message = "";
                if (model.IntervalStart >= model.IntervalEnd)
                {
                    message = "Datumam No ir jābūt lielākam par Datums Līdz";
                    return new JsonResult() { Data = new { IsValid = true, Message = message } };
                }
               
                try
                {
                    var importService = new OutlookItemImportService(this.dataContext, this.CurrentUser)
                    {
                        IntervalStart = model.IntervalStart,
                        IntervalEnd = model.IntervalEnd
                    };

                    var result = importService.ImportOutlookCalendarItems();
                    switch (result.Data)
                    {
                        case OutlookItemImportServiceResult.None:
                            break;
                        case OutlookItemImportServiceResult.Ok:
                            message = "Uzdevumu imports veiksmīgi pabeigts!";
                            break;
                        case OutlookItemImportServiceResult.OkWithWarnings:
                            message = "Uzdevumu imports pabeigts ar paziņojumiem!";
                            break;
                        case OutlookItemImportServiceResult.Error:
                            message = "Uzdevumu imports beidzies ar kļūdu!";
                            break;
                        case OutlookItemImportServiceResult.NotImported:
                            message = "Uzdevumu imports netika veikts!";
                            break;
                    }

                    if (result.Messages != null && result.Messages.Length > 0)
                        message = string.Concat(message, Environment.NewLine, string.Join(" ", result.Messages.Select(m => string.Concat(m.Severity.GetDescription(), ": ", m.Text))));
                }
                catch (Exception ex)
                {
                    message = "Uzdevumu imports beidzies ar kļūdu! " + ex.Message;
                }

                return new JsonResult() { Data = new { IsValid = true, Message = message } };
            }
            else
            {
                return new JsonResult() { Data = new { IsValid = false, Message = string.Join("; ", ModelState.Values.SelectMany(o => o.Errors).Select(x => x.ErrorMessage)) } };
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.dataContext.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Populates the drop down lists in CreateEdit form, which take data from database
        /// </summary>
        /// <param name="model"></param>
        public void PopulateDropDownLists(WorkItemDataInputModel model)
        {
            model.Categories = this.dataContext.Categories.Where(o => o.CreatedByUserId == this.CurrentUser.UserId).ToList();
        }

        /// <summary>
        /// Creates recurring items from a given reccurence pattern and time period
        /// </summary>
        /// <param name="workItem">The work item</param>
        /// <param name="model">The model</param>
        public void CreateOccurrences(WorkItem workItem, WorkItemDataInputModel model)
        {
            var occurrenceService = new OccurrenceService();
            var occurrenceDates = occurrenceService.GetOccurrenceDates(workItem);
            workItem.RecurringItems = new List<RecurringItem>();
            foreach (var date in occurrenceDates)
            {
                var originalDate = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemStart.Value.Hour, model.RecurringItemStart.Value.Minute, model.RecurringItemStart.Value.Second);
                if (!this.dataContext.RecurringItems.Any(r => r.OriginalDate == originalDate))
                {
                    workItem.RecurringItems.Add(new RecurringItem
                    {
                        Start = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemStart.Value.Hour, model.RecurringItemStart.Value.Minute, model.RecurringItemStart.Value.Second),
                        End = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemEnd.Value.Hour, model.RecurringItemEnd.Value.Minute, model.RecurringItemEnd.Value.Second),
                        OriginalDate = originalDate,
                        Exception = false,
                        Subject = workItem.Subject,
                        Body = workItem.Body,
                        Location = workItem.Location,
                        UpdatedAt = DateTime.Now
                    });
                }
            }
        }

        /// <summary>
        /// Removes all of the recurring items for a work item
        /// </summary>
        /// <param name="workItem">The work item</param>
        public void RemoveRecurringItems(WorkItem workItem)
        {
            foreach (var recurringItem in workItem.RecurringItems.ToList())
            {
                this.dataContext.RecurringItems.Remove(recurringItem);
            }
        }
    }
}