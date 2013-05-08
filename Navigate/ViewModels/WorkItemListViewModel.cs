using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.ViewModels
{
    public class WorkItemListViewModel
    {
        public long Id { get; set; }

        public string Subject { get; set; }

        public string Location { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public bool isCompleted { get; set; }
    }
}