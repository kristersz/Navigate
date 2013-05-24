using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum WorkItemType
    {
        None = 0,

        /// <summary>
        /// A type of work item that has a deadline and estimated time of completion
        /// </summary>
        [Description("Task")]
        Task = 1,

        /// <summary>
        /// A type of work item that has a start and end time
        /// </summary>
        [Description("Appointment")]
        Appointment = 2,
    }
}