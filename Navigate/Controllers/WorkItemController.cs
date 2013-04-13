using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Navigate.Models;
using WebMatrix.WebData;

namespace Navigate.Controllers
{   
    [Authorize]
    public class WorkItemController : BaseController
    {
        private NavigateDb db = new NavigateDb();

        //
        // GET: /WorkItem/

        public ActionResult Index(string date = null)
        {

            var currentUserId = this.CurrentUser.UserId;
            var today = DateTime.Now.Date;

            if (date == null)
            {
                var workItems =
                    from r in db.WorkItems
                    where r.CreatedByUserId == currentUserId && r.Date == today
                    orderby r.Priority descending
                    select r;

                ViewBag.PageTitle = "Visi ieraksti";
                return View(workItems.ToList());
            }
            else
            {
                DateTime dt = Convert.ToDateTime(date);

                var workItems =
                    from r in db.WorkItems
                    where r.CreatedByUserId == currentUserId && r.Date == dt
                    orderby r.Priority descending
                    select r;

                ViewBag.PageTitle = "Ieraksti par " + dt.ToString("yyyy-MM-dd");
                return View(workItems.ToList());
            }
        }

        //
        // GET: /WorkItem/Details/5

        public ActionResult Details(int id = 0)
        {
            WorkItem workitem = db.WorkItems.Find(id);
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
            var types =
                from r in db.WorkItemTypes
                select new { Id = r.Id, Type = r.Type };
            ViewBag.WorkItemTypes = types.ToList();
            return View();
        }

        //
        // POST: /WorkItem/Create

        [HttpPost]
        public ActionResult Create(WorkItem workitem)
        {
            var types =
            from r in db.WorkItemTypes
            select new { Id = r.Id, Type = r.Type };
            ViewBag.WorkItemTypes = types.ToList();
            if (ModelState.IsValid)
            {
                db.WorkItems.Add(workitem);
                workitem.CreatedByUserId = this.CurrentUser.UserId;
                workitem.UpdatedByUserId = this.CurrentUser.UserId;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(workitem);
        }

        //
        // GET: /WorkItem/Edit/5

        public ActionResult Edit(int id = 0)
        {
            WorkItem workitem = db.WorkItems.Find(id);
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
                db.Entry(workitem).State = EntityState.Modified;
                workitem.UpdatedByUserId = this.CurrentUser.UserId;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(workitem);
        }

        //
        // GET: /WorkItem/Delete/5

        public ActionResult Delete(int id = 0)
        {
            WorkItem workitem = db.WorkItems.Find(id);
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
            WorkItem workitem = db.WorkItems.Find(id);
            db.WorkItems.Remove(workitem);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}