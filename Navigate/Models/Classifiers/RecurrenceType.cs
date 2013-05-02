using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum RecurrenceType
    {
        [Display(Name = "Daily")]
        Daily = 0,

        [Display(Name = "Weekly")]
        Weekly = 1,

        [Display(Name = "Monthly")]
        Monthly = 2,

        [Display(Name = "Monthly2")]
        MonthNth = 3,

        [Display(Name = "Yearly")]
        Yearly = 4,

        [Display(Name = "Yearly2")]
        YearNth = 5,
    }
}