using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Navigate.Models.Classifiers
{
    public enum WorkItemPriority
    {
        None = 0,

        /// <summary>
        /// Task of exceptional priority
        /// </summary>
        //[Display(Name = "Exceptional")]
        [Description("Exceptional")]
        ExceptionalPriority = 1,

        /// <summary>
        /// Task of high priority
        /// </summary>
        [Description("High")]
        HighPriority = 2,

        /// <summary>
        /// Task of normal priority
        /// </summary>
        [Description("Normal")]
        NormalPriority = 3,

        /// <summary>
        /// Task of low priority
        /// </summary>
        [Description("Low")]
        LowPriority = 4,
    }
}