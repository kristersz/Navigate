using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum RecurrenceType
    {
        /// <summary>
        /// Occurrs every other day
        /// </summary>
        [Display(Name = "Katru dienu")]
        Daily = 0,

        /// <summary>
        /// Occurrs every other week
        /// </summary>
        [Display(Name = "Katru nedēļu")]
        Weekly = 1,

        /// <summary>
        /// Occurrs every other month on a specified day of month
        /// </summary>
        [Display(Name = "Katra mēneša norādītajā datumā")]
        Monthly = 2,

        /// <summary>
        /// Occurrs every other month of a specified day of week, for example, every 2 months on the third wednesday
        /// </summary>
        [Display(Name = "Katra mēneša norādītajā dienā")]
        MonthNth = 3,

        /// <summary>
        /// Occurrs every other year on a specified day of month
        /// </summary>
        [Display(Name = "Katra gada norādītajā datumā")]
        Yearly = 4,

        /// <summary>
        /// Occurrs every other year on a specified day of week, for example, every year on the last weekend day of March
        /// </summary>
        [Display(Name = "Katra gada norādītajā dienā")]
        YearNth = 5,
    }
}