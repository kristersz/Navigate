using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    [Flags]
    public enum DayOfWeekMask
    {
        None = 0,

        [Description("Sunday")]
        Sunday = 1,

        [Description("Monday")]
        Monday = 2,

        [Description("Tuesday")]
        Tuesday = 4,

        [Description("Wednesday")]
        Wednesday = 8,

        [Description("Thursday")]
        Thursday = 16,

        [Description("Friday")]
        Friday = 32,

        [Description("Saturday")]
        Saturday = 64,

        [Description("Day")]
        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday,

        [Description("Weekday")]
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        [Description("Weekend day")]
        Weekend = Sunday | Saturday,
    }
}