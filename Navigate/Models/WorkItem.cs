using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    public class WorkItem
    {
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

        public int Priority { get; set; }

        [Display(Name = "Papildus informācija")] 
        public string AdditionalInfo { get; set; }
    }
}