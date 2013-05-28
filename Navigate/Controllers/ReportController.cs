using Navigate.Models;
using Navigate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    [Authorize]
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
            var output = this.dataContext.WorkItems.Where(o => o.CreatedByUserId == this.CurrentUser.UserId);

            if (!String.IsNullOrWhiteSpace(dateFrom))
            {
                dtFrom = DateTime.Parse(dateFrom);
                output = output.Where(o => o.EndDateTime >= dtFrom);
            }

            if (!String.IsNullOrWhiteSpace(dateTo))
            {
                dtTo = DateTime.Parse(dateTo);
                output = output.Where(o => o.EndDateTime <= dtTo);
            }
            
            var model = new ReportViewModel();
            var countOfNonRecurringItems = output.Count(x => x.isRecurring == false);
            var countOfOccurrences = 0;
            var countOfNonRecurringCompleted = output.Where(o => o.isRecurring == false).Count(x => x.isCompleted == true);
            var countOfOccurrencesCompleted = 0;
            var countOfNonRecurringCompletedInTime = output.Where(o => o.isRecurring == false).Count(x => x.isCompleted == true && x.CompletedAt <= x.EndDateTime);
            var countOfOccurrencesCompletedInTime = 0;
            foreach (var recurringItem in output.Where(x => x.isRecurring == true))
            {
                countOfOccurrences += recurringItem.RecurringItems.Count();
                countOfOccurrencesCompleted += recurringItem.RecurringItems.Count(x => x.isCompleted == true);
                countOfOccurrencesCompletedInTime += recurringItem.RecurringItems.Count(x => x.isCompleted == true && x.CompletedAt <= x.End);
            }

            model.CountOfPlannedItems = countOfNonRecurringItems + countOfOccurrences;
            model.CountOfCompletedItems = countOfNonRecurringCompleted + countOfOccurrencesCompleted;
            model.CountOfCompletedInTime = countOfNonRecurringCompletedInTime + countOfOccurrencesCompletedInTime;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Report", model);
            }

            return View(model);
        }
    }
}
