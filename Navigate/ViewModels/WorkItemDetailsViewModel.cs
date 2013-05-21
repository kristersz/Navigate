using Navigate.Models;
using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.ViewModels
{
    public class WorkItemDetailsViewModel
    {
        public WorkItemDetailsViewModel()
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

        [Display(Name = "Ilgums")]
        public double Duration { get; set; }

        [Display(Name = "Papildus informācija")]
        public string Body { get; set; }

        [Display(Name = "Uzdevuma tips")]
        public WorkItemType WorkItemType { get; set; }

        public bool isCompleted { get; set; }

        public bool isRecurring { get; set; }

        public WorkItemPriority? Priority { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<Category> Categories { get; set; }
        public ICollection<RecurringItem> RecurringItems { get; set; }
    }
}