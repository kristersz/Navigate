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
        /// <summary>
        /// Gets or sets the id
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the category name
        /// </summary>
        [Display(Name = "Kategorijas nosaukums")]
        [Required(ErrorMessage = "Kategorijas nosaukums ir obligāts lauks")]
        [MaxLength(140, ErrorMessage = "Kategorijas nosaukums nevar būt garāks par 140 simboliem.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category description
        /// </summary>
        [Display(Name = "Kategorijas apraksts")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the user id, that created the category
        /// </summary>
        public int CreatedByUserId { get; set; }

        public ICollection<WorkItem> WorkItems { get; set; }
    }
}