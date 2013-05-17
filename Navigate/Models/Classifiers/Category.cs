using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public class Category
    {
        public int? Id { get; set; }

        [Display(Name = "Kategorija")]
        [Required(ErrorMessage = "Kategorijas nosaukums ir obligāts lauks")]
        [MaxLength(140, ErrorMessage = "Kategorijas nosaukums nevar būt garāks par 140 simboliem.")]
        public string Name { get; set; }

        [Display(Name = "Kategorijas apraksts")]
        public string Description { get; set; }

        public int CreatedByUserId { get; set; }

        public ICollection<WorkItem> WorkItems { get; set; }
    }
}