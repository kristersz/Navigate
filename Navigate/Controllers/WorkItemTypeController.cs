using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Navigate.Models;

namespace Navigate.Controllers
{
    public class WorkItemTypeController : Controller
    {
        private NavigateDb db = new NavigateDb();

        //
        // GET: /WorkItemType/

        public ActionResult Index()
        {
            return View(db.WorkItemTypes.ToList());
        }

        //
        // GET: /WorkItemType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /WorkItemType/Create

        [HttpPost]
        public ActionResult Create(WorkItemType workitemtype)
        {
            if (ModelState.IsValid)
            {
                db.WorkItemTypes.Add(workitemtype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(workitemtype);
        }


        //
        // GET: /WorkItemType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            WorkItemType workitemtype = db.WorkItemTypes.Find(id);
            if (workitemtype == null)
            {
                return HttpNotFound();
            }
            return View(workitemtype);
        }

        //
        // POST: /WorkItemType/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            WorkItemType workitemtype = db.WorkItemTypes.Find(id);
            db.WorkItemTypes.Remove(workitemtype);
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