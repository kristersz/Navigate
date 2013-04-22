using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public class WorkItemType
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Work Item Type")]
        [Required]
        [MaxLength(20, ErrorMessage = "Work item type cannot exceed 20 characters")]
        public string Type { get; set; }
    }
}