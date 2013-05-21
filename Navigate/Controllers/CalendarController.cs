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

        /// <summary>
        /// Gets the events from database for calendar to use
        /// </summary>
        /// <param name="start">Unix timestamp indicating the start date of the time period</param>
        /// <param name="end">Unix timestamp indicating the end date of the time period</param>
        /// <returns>An array of events</returns>
        [HttpGet]
        public JsonResult GetEvents(double start, double end)
        {
            var startDateTime = FromUnixTimestamp(start);
            var endDateTime = FromUnixTimestamp(end);

            var events = this.dataContext.WorkItems
                .Where(e => e.CreatedByUserId == this.CurrentUser.UserId && e.isCompleted == false)
                .ToList();
            var eventList = new List<object>();

            foreach (var e in events)
            {
                if (e.isRecurring == false &&(e.StartDateTime >= startDateTime && e.EndDateTime <= endDateTime))
                {
                    eventList.Add(
                        new
                        {
                            id = e.Id,
                            title = e.Subject,
                            start = e.StartDateTime.Value.ToString("yyyy-MM-dd HH:mm"),
                            end = e.EndDateTime.ToString("yyyy-MM-dd HH:mm"),
                            allDay = e.AllDayEvent,
                            url = "/WorkItem/Details/" + e.Id.ToString() + "/"
                        });
                }
                else 
                {
                    //Display each of the occurrence in the calendar as part of a recurring item group by assigning the same id to each event
                    foreach (var recurringItem in e.RecurringItems.Where(o => o.Start >= startDateTime && o.End <= endDateTime && o.isCompleted == false))
                    {
                        eventList.Add(
                            new
                            {
                                id = e.Id,
                                title = e.Subject,
                                start = recurringItem.Start.ToString("yyyy-MM-dd HH:mm"),
                                end = recurringItem.End.ToString("yyyy-MM-dd HH:mm"),
                                allDay = false,
                                url = "/RecurringItem/Details/" + recurringItem.Id.ToString() + "/"
                            });
                    }
                }

            }
            return Json(eventList.ToArray(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Converts unix timestamp to a datetime object
        /// </summary>
        /// <param name="timestamp">The unix timestamp</param>
        /// <returns>The datetime object</returns>
        private static DateTime FromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }
    }
}
