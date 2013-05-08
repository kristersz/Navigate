using Navigate.Models;
using Navigate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    public class CalendarController : BaseController
    {
        private readonly EventModel eventModel;

        public CalendarController()
        {
            eventModel = new EventModel();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetEvents(double start, double end)
        {
            var startDateTime = FromUnixTimestamp(start);
            var endDateTime = FromUnixTimestamp(end);

            var events = this.dataContext.WorkItems
                .Where(o => o.StartDateTime >= startDateTime && o.EndDateTime <= endDateTime)
                .ToList();
            var eventList = new List<object>();

            foreach (var e in events)
            {
                eventList.Add(
                    new
                    {
                        id = e.Id,
                        title = e.Subject,
                        start = e.StartDateTime.ToString("yyyy-MM-dd HH:mm"),
                        end = e.EndDateTime.ToString("yyyy-MM-dd HH:mm"),
                        allDay = false,
                        url = "/WorkItem/Details/" + e.Id.ToString() + "/"
                    });

            }
            return Json(eventList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        private static DateTime FromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }
}
