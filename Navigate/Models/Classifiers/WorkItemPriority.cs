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
        [Description("Ļoti augsta")]
        ExceptionalPriority = 1,

        /// <summary>
        /// Task of high priority
        /// </summary>
        [Description("Augsta")]
        HighPriority = 2,

        /// <summary>
        /// Task of normal priority
        /// </summary>
        [Description("Parasta")]
        NormalPriority = 3,

        /// <summary>
        /// Task of low priority
        /// </summary>
        [Description("Zema")]
        LowPriority = 4,

        /// <summary>
        /// Task of very low priority
        /// </summary>
        [Description(" Ļoti zema")]
        VeryLowPriority = 5,
    }
}