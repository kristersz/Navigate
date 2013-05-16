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
                        Priority = r.Priority,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.UpdatedAt,
                        Categories = r.Categories
                    }
                );

            IEnumerable<SelectListItem> categoriesList = new List<SelectListItem>();
            categoriesList = this.dataContext.Categories
                .Where(o => o.CreatedByUserId == this.CurrentUser.UserId)
                .ToList()
                .Select(o => new SelectListItem
                    {
                        Value = o.ID.ToString(),
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
                workItems = workItems.Where(r => r.Categories.Any(o => o.ID == catId));
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
                    workItems = workItems.Where(r => r.EndDateTime < DateTime.Today);
                    break;
                case "today":
                    workItems = workItems.Where(r => EntityFunctions.TruncateTime(r.EndDateTime) == EntityFunctions.TruncateTime(DateTime.Now));
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

        public ActionResult Create(string start = null, string end = null)
        {
            var model = new WorkItemDataInputModel();
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end))
            {
                DateTime dtStart = DateTime.ParseExact(start, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                DateTime dtEnd = DateTime.ParseExact(end, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
                model.StartDate = dtStart;
                model.EndDate = dtEnd;
            }
            populateDropDownLists(model);
            return View(model);
        }

        //
        // POST: /WorkItem/Create

        [HttpPost]
        public ActionResult Create(WorkItemDataInputModel model)
        {
            populateDropDownLists(model);
            if (model.WorkItemType == WorkItemType.None)
            {
                ModelState.AddModelError("", "Uzdevuma tips ir obligāts lauks");
                return View(model);
            }
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

                    var occurrenceService = new OccurrenceService();
                    var occurrenceDates = occurrenceService.GetOccurrenceDates(workItem);
                    workItem.RecurringItems = new List<RecurringItem>();
                    foreach (var date in occurrenceDates)
                    {
                        workItem.RecurringItems.Add(new RecurringItem
                        {
                            Start = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemStart.Value.Hour, model.RecurringItemStart.Value.Minute, model.RecurringItemStart.Value.Second),
                            End = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemEnd.Value.Hour, model.RecurringItemEnd.Value.Minute, model.RecurringItemEnd.Value.Second),
                            OriginalDate = new DateTime(date.Year, date.Month, date.Day, model.RecurringItemStart.Value.Hour, model.RecurringItemStart.Value.Minute, model.RecurringItemStart.Value.Second),
                            Subject = workItem.Subject,
                            Body = workItem.Body,
                            Location = workItem.Location,
                            UpdatedAt = DateTime.Now
                        });
                    }
                }

                this.dataContext.WorkItems.Add(workItem);
                this.dataContext.SaveChanges();

                //var scheduler = new ReminderScheduler();
                //scheduler.ScheduleReminder(workItem);

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
        public ActionResult Edit(WorkItemDataInputModel model)
        {
            populateDropDownLists(model);
            if (ModelState.IsValid)
            {
                WorkItem workItem = this.dataContext.WorkItems.Where(o => o.Id == model.WorkItemId).FirstOrDefault();
                if (workItem != null)
                {
                    model.UpdateWorkItem(workItem);
                    workItem.UpdatedAt = DateTime.Now;
                    workItem.UpdatedByUserId = this.CurrentUser.UserId;
                    this.dataContext.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(model);
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

        [HttpPost]
        public JsonResult ChangeStatus(int id = 0)
        {
            WorkItem workItem = this.dataContext.WorkItems.Find(id);
            var message = "";
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
                    message = "Uzdevums "+ workItem.Subject.ToString() + " veiksmīgi tika atzīmēts kā izpildīts";
                }
                else
                {
                    workItem.isCompleted = false;
                    message = "Uzdevums " + workItem.Subject.ToString() + " veiksmīgi tika atzīmēts kā neizpildīts";
                }
            }
            this.dataContext.SaveChanges();
            return new JsonResult() { Data = new { IsValid = true, Message = message } };

        }

        [HttpPost]
        public JsonResult GetOutlookCalendarItems(OutlookSettingsInputModel model)
        {
            string message = "";
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

            return new JsonResult() { Data = new { Message = message } };
        }

        protected override void Dispose(bool disposing)
        {
            this.dataContext.Dispose();
            base.Dispose(disposing);
        }

        public void populateDropDownLists(WorkItemDataInputModel model)
        {
            model.Categories = this.dataContext.Categories.Where(o => o.CreatedByUserId == this.CurrentUser.UserId).ToList();
        }
    }
}