using DayPilot.Web.Mvc;
using DayPilot.Web.Mvc.Enums;
using DayPilot.Web.Mvc.Events.Calendar;
using Navigate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    public class CalendarController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Backend()
        {
            return new Dpc().CallBack(this);
        }

        class Dpc : DayPilotCalendar
        {
            NavigateDb db = new NavigateDb();

            protected override void OnInit(InitArgs e)
            {
                Update(CallBackUpdateType.Full);
            }

            protected override void OnEventResize(EventResizeArgs e)
            {
                var id = Convert.ToInt32(e.Id);
                var toBeResized = (from ev in db.WorkItems where ev.Id == id select ev).First();
                toBeResized.StartDateTime = e.NewStart;
                toBeResized.EndDateTime = e.NewEnd;
                db.SaveChanges();
                Update();
            }

            protected override void OnEventMove(EventMoveArgs e)
            {
                var id = Convert.ToInt32(e.Id);
                var toBeResized = (from ev in db.WorkItems where ev.Id == id select ev).First();
                toBeResized.StartDateTime = e.NewStart;
                toBeResized.EndDateTime = e.NewEnd;
                db.SaveChanges();
                Update();
            }

            protected override void OnEventClick(EventClickArgs e)
            {
                var id = Convert.ToInt32(e.Id);
                
            }
            
            protected override void OnTimeRangeSelected(TimeRangeSelectedArgs e)
            {
                var toBeCreated = new WorkItem();
                toBeCreated.StartDateTime = e.Start;
                toBeCreated.EndDateTime = e.End;
                toBeCreated.Subject = (string)e.Data["name"];
                toBeCreated.WorkItemTypeId = 2;
                toBeCreated.Location = (string)e.Data["location"];
                toBeCreated.CreatedAt = DateTime.Now;
                toBeCreated.UpdatedAt = DateTime.Now;
                toBeCreated.CreatedByUserId = 1;
                toBeCreated.UpdatedByUserId = 1;
                db.WorkItems.Add(toBeCreated);
                db.SaveChanges();
                Update();
            }

            protected override void OnFinish()
            {
                if (UpdateType == CallBackUpdateType.None)
                {
                    return;
                }

                Events = from ev in db.WorkItems select ev;

                DataIdField = "Id";
                DataTextField = "Subject";
                DataStartField = "StartDateTime";
                DataEndField = "EndDateTime";
            }
        }
    }
}
