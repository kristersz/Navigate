using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    public class RecurringItem
    {
        [Key]
        public long Id { get; set; }

        public DateTime OriginalDate { get; set; }

        public long WorkItemId { get; set; }

        [ForeignKey("WorkItemId")]
        public virtual WorkItem WorkItem { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string Subject { get; set; }

        public string Location { get; set; }

        public string Body { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}