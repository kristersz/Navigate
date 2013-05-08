using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public class Category
    {
        public int? ID { get; set; }

        [Display(Name = "Kategorija")]
        [Required(ErrorMessage = "Kategorijas nosaukums ir obligāts lauks")]
        [MaxLength(140, ErrorMessage = "Kategorijas nosaukums nevar būt garāks par 140 simboliem.")]
        public string Name { get; set; }

        public ICollection<WorkItem> WorkItems { get; set; }
    }
}