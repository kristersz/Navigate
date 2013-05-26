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
            // FullCalendar passes Unix timestamps in the query string indicating the time period that it needs the events for
            // so we first convert these timestamps to datetime objects and then use them to query the database for events that we need to display
            var startDateTime = FromUnixTimestamp(start);
            var endDateTime = FromUnixTimestamp(end);

            var events = this.dataContext.WorkItems
                .Where(e => e.CreatedByUserId == this.CurrentUser.UserId && e.isCompleted == false)
                .ToList();
            var eventList = new List<object>();

            foreach (var e in events)
            {
                // we display non-recurring items as they are
                // for recurring work items however, we display each of their occurrences 
                if (e.isRecurring == false && (e.StartDateTime >= startDateTime && e.EndDateTime <= endDateTime))
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
                    foreach (var recurringItem in e.RecurringItems.Where(o => o.Start >= startDateTime && o.End <= endDateTime))
                    {
                        eventList.Add(
                            new
                            {
                                id = e.Id,
                                title = recurringItem.Subject,
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

        /// <summary>
        /// Handles the drag and drop event of the calendar UI
        /// </summary>
        /// <param name="model">The event model</param>
        /// <returns>JSON Result</returns>
        [HttpPost]
        public JsonResult DropEvent(EventModel model)
        {
            var workItem = this.dataContext.WorkItems.Find(model.Id);
            if (workItem == null)
            {
                return new JsonResult() { Data = new { IsValid = false, Message = "Uzdevums netika atrasts" } };

            }

            if (!workItem.isRecurring)
            {
                workItem.StartDateTime = workItem.StartDateTime.Value.AddDays(model.dayDelta).AddMinutes(model.minuteDelta);
                workItem.EndDateTime = workItem.EndDateTime.AddDays(model.dayDelta).AddMinutes(model.minuteDelta);
                workItem.AllDayEvent = model.AllDayEvent;
                workItem.UpdatedAt = DateTime.Now;               
            }
            else
            {
                foreach (var recurringItem in workItem.RecurringItems)
                {
                    recurringItem.Start = recurringItem.Start.AddDays(model.dayDelta).AddMinutes(model.minuteDelta);
                    recurringItem.End = recurringItem.End.AddDays(model.dayDelta).AddMinutes(model.minuteDelta);
                    recurringItem.UpdatedAt = DateTime.Now;                    
                }
            }

            this.dataContext.SaveChanges();
            return new JsonResult() { Data = new { IsValid = true, Message = "Izmaiņas veiksmīgi saglabātas" } };
        }

        /// <summary>
        /// Handles the resize event of the calendar UI
        /// </summary>
        /// <param name="model">The event model</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        public JsonResult ResizeEvent(EventModel model)
        {
            var workItem = this.dataContext.WorkItems.Find(model.Id);
            if (workItem == null)
            {
                return new JsonResult() { Data = new { IsValid = false, Message = "Uzdevums netika atrasts" } };

            }

            if (!workItem.isRecurring)
            {
                workItem.StartDateTime = workItem.StartDateTime.Value.AddDays(model.dayDelta);
                workItem.EndDateTime = workItem.EndDateTime.AddDays(model.dayDelta).AddMinutes(model.minuteDelta);
                workItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                foreach (var recurringItem in workItem.RecurringItems)
                {
                    recurringItem.Start = recurringItem.Start.AddDays(model.dayDelta);
                    recurringItem.End = recurringItem.End.AddDays(model.dayDelta).AddMinutes(model.minuteDelta);
                    recurringItem.UpdatedAt = DateTime.Now;
                }
            }

            this.dataContext.SaveChanges();
            return new JsonResult() { Data = new { IsValid = true, Message = "Izmaiņas veiksmīgi saglabātas" } };
        }
    }
}
