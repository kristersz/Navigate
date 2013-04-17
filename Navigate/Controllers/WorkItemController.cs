using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Navigate.Models;
using WebMatrix.WebData;
using Navigate.ViewModels;

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

            var workItems =
                from r in this.dataContext.WorkItems
                where r.CreatedByUserId == currentUserId
                orderby r.Priority descending
                select r;

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
            return View(model);
        }

        //
        // POST: /WorkItem/Create

        [HttpPost]
        public ActionResult Create(WorkItemCreateViewModel model)
        {
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

        protected override void Dispose(bool disposing)
        {
            this.dataContext.Dispose();
            base.Dispose(disposing);
        }
    }
}