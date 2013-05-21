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

        [Description("Task")]
        Task = 1,

        [Description("Appointment")]
        Appointment = 2,
    }
}