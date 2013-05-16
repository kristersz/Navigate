using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Navigate.Controllers
{
    public class ClassifierController : BaseController
    {
        /// <summary>
        /// Displays a list of all editable classifiers
        /// </summary>
        /// <returns>View</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Lists all categories
        /// </summary>
        /// <returns></returns>
        public ActionResult ListCategories()
        {
            var categories = this.dataContext.Categories.Where(o => o.CreatedByUserId == this.CurrentUser.UserId).ToList();
            return View(categories);
        }

        /// <summary>
        /// Creates a category
        /// </summary>
        /// <param name="model">Category model</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        public JsonResult CreateCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                model.Name = model.Name.Trim();
                if (string.IsNullOrEmpty(model.Name))
                    return new JsonResult() { Data = new { IsValid = false, Message = "Kategorijas nosaukums ir obligāts lauks." } };

                var newCategory = new Category { Name = model.Name, Description = model.Description, CreatedByUserId = this.CurrentUser.UserId };
                this.dataContext.Categories.Add(newCategory);
                this.dataContext.SaveChanges();

                return new JsonResult() { Data = new { IsValid = true, Message = "Kategorija veiksmīgi pievienota" } };
            }

            return new JsonResult() { Data = new { IsValid = false, Message = string.Join("; ", ModelState.Values.SelectMany(o => o.Errors).Select(x => x.ErrorMessage)) } };
        }

        /// <summary>
        /// Deletes a category by its id parameter
        /// </summary>
        /// <param name="categoryId">Category id</param>
        /// <returns>JSON result</returns>
        [HttpPost]
        public JsonResult DeleteCategory(int categoryId = 0)
        {
            if (categoryId == 0)
                return new JsonResult() { Data = new { IsValid = false, Message = "Kategorija netika atrasta" } };

            var category = this.dataContext.Categories.Where(o => o.ID == categoryId).FirstOrDefault();
            if (category == null)
                return new JsonResult() { Data = new { IsValid = false, Message = "Kategorija netika atrasta" } };

            this.dataContext.Categories.Remove(category);
            this.dataContext.SaveChanges();

            return new JsonResult() { Data = new { IsValid = true, Message = "Kategorija veiksmīgi izdzēsta" } };
        }
    }
}
