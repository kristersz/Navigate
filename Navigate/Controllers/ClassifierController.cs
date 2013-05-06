using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    public class ClassifierController : BaseController
    {
        //
        // GET: /Classifier/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListCategories()
        {
            var categories = this.dataContext.Categories.ToList();
            return View(categories);
        }

    }
}
