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
    [Authorize]
    public class RecurringItemController : BaseController
    {

        //
        // GET: /RecurringItem/

        public ActionResult Index()
        {
            var recurringitems = this.dataContext.RecurringItems.Include(r => r.WorkItem);
            return View(recurringitems.ToList());
        }

        //
        // GET: /RecurringItem/Details/5

        public ActionResult Details(long id = 0)
        {
            RecurringItem recurringitem = this.dataContext.RecurringItems.Find(id);
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
            RecurringItem recurringitem = this.dataContext.RecurringItems.Find(id);
            if (recurringitem == null)
            {
                return HttpNotFound();
            }
            ViewBag.WorkItemId = new SelectList(this.dataContext.WorkItems, "Id", "Subject", recurringitem.WorkItemId);
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
                this.dataContext.Entry(recurringitem).State = EntityState.Modified;
                this.dataContext.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.WorkItemId = new SelectList(this.dataContext.WorkItems, "Id", "Subject", recurringitem.WorkItemId);
            return View(recurringitem);
        }

        //
        // GET: /RecurringItem/Delete/5

        public ActionResult Delete(long id = 0)
        {
            RecurringItem recurringitem = this.dataContext.RecurringItems.Find(id);
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
            RecurringItem recurringitem = this.dataContext.RecurringItems.Find(id);
            this.dataContext.RecurringItems.Remove(recurringitem);
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