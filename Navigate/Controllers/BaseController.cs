﻿using Navigate.Models;
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

        //Get the current user
        public UserProfile CurrentUser
        {
            get
            {
                if (this.User == null || this.User.Identity == null)
                    return null;

                return this.db.UserProfiles.First(o => o.UserName == User.Identity.Name);
            }
        }
    }
}