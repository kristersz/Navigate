using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
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

        public string Subject { get; set; }

        public string Location { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public bool isCompleted { get; set; }

        public WorkItemPriority? Priority { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}