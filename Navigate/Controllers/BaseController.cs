using Navigate.Models;
using Navigate.Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    public abstract class BaseController : Controller
    {
        private NavigateDb db = new NavigateDb();

        private ReminderScheduler _scheduler = new ReminderScheduler();

        protected NavigateDb dataContext
        {
            get { return this.db; }
        }

        protected ReminderScheduler scheduler
        {
            get { return this._scheduler; }
        }

        //Gets the current user by its name
        public UserProfile CurrentUser
        {
            get
            {
                if (this.User == null || this.User.Identity == null)
                    return null;

                return this.dataContext.UserProfiles.First(o => o.UserName == User.Identity.Name);
            }
        }
    }
}
