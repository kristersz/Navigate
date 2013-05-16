using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum RecurrenceType
    {
        [Display(Name = "Katru dienu")]
        Daily = 0,

        [Display(Name = "Katru nedēļu")]
        Weekly = 1,

        [Display(Name = "Katra mēneša norādītajā datumā")]
        Monthly = 2,

        [Display(Name = "Katra mēneša norādītajā dienā")]
        MonthNth = 3,

        [Display(Name = "Katra gada norādītajā datumā")]
        Yearly = 4,

        [Display(Name = "Katra gada norādītajā dienā")]
        YearNth = 5,
    }
}