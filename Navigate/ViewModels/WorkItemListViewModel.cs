using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Navigate.ViewModels
{
    public class WorkItemListViewModel
    {
        public WorkItemListViewModel()
        {
            
        }

        public long Id { get; set; }

        [Display(Name = "Nosaukums")]
        public string Subject { get; set; }

        [Display(Name = "Atrašanās vieta")]
        public string Location { get; set; }

        [Display(Name = "Sākuma laiks")]
        public DateTime? StartDateTime { get; set; }

        [Display(Name = "Beigu laiks")]
        public DateTime EndDateTime { get; set; }

        public bool isCompleted { get; set; }

        public bool isRecurring { get; set; }

        public WorkItemPriority? Priority { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public RecurringItem NextRecurringItem { get; set; }

        public ICollection<Category> Categories { get; set; }
        public ICollection<RecurringItem> RecurringItems { get; set; }

    }
}