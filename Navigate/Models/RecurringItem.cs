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
        public RecurringItem()
        {
            this.isCompleted = false;
        }

        [Key]
        public long Id { get; set; }

        public DateTime OriginalDate { get; set; }

        public long WorkItemId { get; set; }

        [ForeignKey("WorkItemId")]
        public virtual WorkItem WorkItem { get; set; }

        [Display(Name = "Sākums")]
        [DataType(DataType.DateTime)]
        public DateTime Start { get; set; }

        [Display(Name = "Beigas")]
        [DataType(DataType.DateTime)]
        public DateTime End { get; set; }

        public double Duration { get; set; }

        [Display(Name = "Nosaukums")]
        public string Subject { get; set; }

        [Display(Name = "Atrašanās vieta")]
        public string Location { get; set; }

        [Display(Name = "Informācija")]
        public string Body { get; set; }

        public string Origin { get; set; }

        public bool isCompleted { get; set; }

        public bool Exception { get; set; }

        [Display(Name = "Pabeigts")]
        public DateTime? CompletedAt { get; set; }

        [Display(Name = "Rediģēts")]
        public DateTime UpdatedAt { get; set; }
    }
}