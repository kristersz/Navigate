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
    public class RecurringItemController : Controller
    {
        private NavigateDb db = new NavigateDb();

        //
        // GET: /RecurringItem/

        public ActionResult Index()
        {
            var recurringitems = db.RecurringItems.Include(r => r.WorkItem);
            return View(recurringitems.ToList());
        }

        //
        // GET: /RecurringItem/Details/5

        public ActionResult Details(long id = 0)
        {
            RecurringItem recurringitem = db.RecurringItems.Find(id);
            if (recurringitem == null)
            {
                return HttpNotFound();
            }
            return View(recurringitem);
        }

        //
        // GET: /RecurringItem/Edit/5

        public ActionResult Edit(long id = 0)
        {
            RecurringItem recurringitem = db.RecurringItems.Find(id);
            if (recurringitem == null)
            {
                return HttpNotFound();
            }
            ViewBag.WorkItemId = new SelectList(db.WorkItems, "Id", "Subject", recurringitem.WorkItemId);
            return View(recurringitem);
        }

        //
        // POST: /RecurringItem/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(RecurringItem recurringitem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recurringitem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.WorkItemId = new SelectList(db.WorkItems, "Id", "Subject", recurringitem.WorkItemId);
            return View(recurringitem);
        }

        //
        // GET: /RecurringItem/Delete/5

        public ActionResult Delete(long id = 0)
        {
            RecurringItem recurringitem = db.RecurringItems.Find(id);
            if (recurringitem == null)
            {
                return HttpNotFound();
            }
            return View(recurringitem);
        }

        //
        // POST: /RecurringItem/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            RecurringItem recurringitem = db.RecurringItems.Find(id);
            db.RecurringItems.Remove(recurringitem);
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