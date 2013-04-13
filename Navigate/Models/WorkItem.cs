using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    public class WorkItem
    {
        [Key]
        public long Id { get; set; }

        [Display(Name="Nosaukums")]   
        public string Subject { get; set; }

        [Display(Name = "Atrašanās vieta")] 
        public string Location { get; set; }
        
        [Display(Name = "Datums")] 
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Aptuvenais laiks")] 
        public decimal EstimatedTime { get; set; }

        public int? WorkItemTypeId { get; set; }

        [Display(Name = "Darba uzdevuma tips")]
        [ForeignKey("WorkItemTypeId")]
        public virtual WorkItemType WorkItemType { get; set; }

        [Display(Name = "Prioritāte")]
        [Range(0, 99)]
        public int Priority { get; set; }

        [Display(Name = "Papildus informācija")] 
        public string AdditionalInfo { get; set; }

        public bool isCompleted { get; set; }

        public bool isRecurring { get; set; }

        public int CreatedByUserId { get; set; }

        public int UpdatedByUserId { get; set; }
    }
}