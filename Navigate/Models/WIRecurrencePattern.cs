using Navigate.Models.Classifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    [Table("RecurrencePatterns")]
    public class WIRecurrencePattern
    {
        [Key]
        public long WorkItemId { get; set; }

        public int Interval { get; set; }

        public DayOfWeekMask DayOfWeekMask { get; set; }

        public int DayOfMonth { get; set; }

        public Instance Instance { get; set; }

        public MonthOfYear MonthOfYear { get; set; }

        [ForeignKey("WorkItemId")]
        public virtual WorkItem WorkItem { get; set; }
    }
}