using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum MonthOfYear
    {
        [Description("Janvāris")]
        January = 1,

        [Description("Februāris")]
        February = 2,

        [Description("Marts")]
        March = 3,

        [Description("Aprīlis")]
        April = 4,

        [Description("Maijs")]
        May = 5,

        [Description("Jūnijs")]
        June = 6,

        [Description("Jūlijs")]
        July = 7,

        [Description("Augusts")]
        August = 8,

        [Description("Septembris")]
        September = 9,

        [Description("Oktobris")]
        October = 10,

        [Description("Novembris")]
        November = 11,

        [Description("Decembris")]
        December = 12,
    }
}