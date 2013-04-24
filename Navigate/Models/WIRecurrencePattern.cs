using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    public class WIRecurrencePattern
    {
        [Key]
        public long Id { get; set; }

        public int Interval { get; set; }

        public int DayOfWeekMask { get; set; }

        public int DayOfMonth { get; set; }

        public int Instance { get; set; }

        public int MonthOfYear { get; set; }
    }
}