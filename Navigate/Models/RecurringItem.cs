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

        public long WorkItemId { get; set; }

        [ForeignKey("WorkItemId")]
        public virtual WorkItem WorkItem { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string IndividualLocation { get; set; }

        public string IndividualBody { get; set; }
    }
}