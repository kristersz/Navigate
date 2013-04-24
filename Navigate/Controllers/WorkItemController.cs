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

        public void GetAllCalendarItems()
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
                        workItem.StartDate = item.Start;
                        workItem.EndDate = item.End;
                        workItem.EstimatedTime = item.Duration;
                        workItem.AdditionalInfo = item.Body;
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
                            existingWorkItem.StartDate = item.Start;
                            existingWorkItem.EndDate = item.End;
                            existingWorkItem.EstimatedTime = item.Duration;
                            existingWorkItem.AdditionalInfo = item.Body;
                            existingWorkItem.UpdatedAt = DateTime.Now;
                            existingWorkItem.UpdatedByUserId = this.CurrentUser.UserId;

                            this.dataContext.SaveChanges();
                        }
                    }
                }
                else if (item.IsRecurring)
                {
                    RecurrencePattern recurrencePattern = item.GetRecurrencePattern();
                    OlRecurrenceType recurrenceType = recurrencePattern.RecurrenceType;

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
                        workItem.Subject = item.Subject;
                        workItem.Location = item.Location;
                        workItem.OutlookEntryId = item.EntryID;
                        workItem.StartDate = item.Start;
                        workItem.EndDate = item.End;
                        workItem.EstimatedTime = item.Duration;
                        workItem.AdditionalInfo = item.Body;
                        workItem.WorkItemTypeId = this.dataContext.WorkItemTypes.Where(o => o.Type == "Meeting").FirstOrDefault().Id;
                        workItem.isRecurring = false;
                        workItem.RecurrencePatternId = pattern.Id;
                        workItem.RecurrenceType = RecurrenceType.Monthly;
                        workItem.CreatedByUserId = this.CurrentUser.UserId;
                        workItem.UpdatedByUserId = this.CurrentUser.UserId;

                        this.dataContext.WorkItems.Add(workItem);
                        this.dataContext.SaveChanges();

                        DateTime first = new DateTime(item.Start.Year, item.Start.Month, item.Start.Day, item.Start.Hour, item.Start.Minute, item.Start.Second);
                        DateTime last = new DateTime(item.End.Year, item.End.Month, item.End.Day, item.End.Hour, item.End.Minute, item.End.Second);
                        AppointmentItem recur = null;

                        for (DateTime cur = first; cur <= last; cur = cur.AddDays(1))
                        {
                            try
                            {
                                recur = recurrencePattern.GetOccurrence(cur);
                                ViewBag.RecurSubj = recur.Subject;
                                ViewBag.RecurLoc = recur.Location;
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

        public long? recurrencePattern { get; set; }
    }
}