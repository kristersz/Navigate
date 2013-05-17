using Navigate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    public class ReportController : BaseController
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewReport(string dateFrom = null, string dateTo = null)
        {
            var dtFrom = new DateTime();
            var dtTo = new DateTime();

            if (!String.IsNullOrWhiteSpace(dateFrom))
            {
                dtFrom = DateTime.Parse(dateFrom);
            }
            if (!String.IsNullOrWhiteSpace(dateTo))
            {
                dtTo = DateTime.Parse(dateTo);
            }
            var output = this.dataContext.WorkItems
                .Where(o => o.EndDateTime >= dtFrom && o.EndDateTime <= dtTo)
                .ToList();

            var model = new ReportViewModel();
            model.CountOfPlannedItems = output.Count();
            model.CountOfCompletedItems = output.Where(o => o.isCompleted).Count();
            model.CountOfCompletedInTime = output.Where(o => o.isCompleted && o.CompletedAt <= o.EndDateTime).Count();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Report", model);
            }

            return View(model);
        }
    }
}
