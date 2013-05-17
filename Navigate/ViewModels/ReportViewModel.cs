using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.ViewModels
{
    public class ReportViewModel
    {
        public int CountOfPlannedItems { get; set; }

        public int CountOfCompletedItems { get; set; }

        public int CountOfCompletedInTime { get; set; }
    }
}