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
        [Description("Uzdevums")]
        Task = 1,

        [Description("Sapulce")]
        Appointment = 2,
    }
}