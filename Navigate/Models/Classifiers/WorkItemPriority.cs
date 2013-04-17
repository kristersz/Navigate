using System;
using System.ComponentModel.DataAnnotations;

namespace Navigate.Models
{
    public enum WorkItemPriority
    {

        /// <summary>
        /// Task of exceptional priority
        /// </summary>
        [Display(Name = "Exceptional")]
        ExceptionalPriority = 1,

        /// <summary>
        /// Task of high priority
        /// </summary>
        [Display(Name = "High")]
        HighPriority = 2,

        /// <summary>
        /// Task of normal priority
        /// </summary>
        [Display(Name = "Normal")]
        NormalPriority = 3,

        /// <summary>
        /// Task of low priority
        /// </summary>
        [Display(Name = "Low")]
        LowPriority = 4,
    }
}