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

                var recurrencePattern = model.TransformToRecurrencePattern();
                this.dataContext.WIRecurrencePatterns.Add(recurrencePattern);
                this.dataContext.SaveChanges();

                workItem.WIRecurrencePatternId = recurrencePattern.Id;

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

            //model.AllPriorities = from v in (WorkItemPriority[])(Enum.GetValues(typeof(WorkItemPriority)))
            //                          select new SelectListItem()
            //                          {
            //                              Text = ((DisplayAttribute)(typeof(WorkItemPriority)
            //                                        .GetField(v.ToString()).GetCustomAttributes(typeof(DisplayAttribute), false).First())).Name,
            //                              Value = v.ToString(),
            //                          };
        }
    }
}