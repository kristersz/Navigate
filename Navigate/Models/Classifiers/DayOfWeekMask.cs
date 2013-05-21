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
        [Description("Svētdienā")]
        Sunday = 1,

        [Description("Pirmdienā")]
        Monday = 2,

        [Description("Otrdienā")]
        Tuesday = 4,

        [Description("Trešdienā")]
        Wednesday = 8,

        [Description("Ceturtdienā")]
        Thursday = 16,

        [Description("Piektdienā")]
        Friday = 32,

        [Description("Sestdienā")]
        Saturday = 64,

        [Description("Dienā")]
        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday,

        [Description("Darbadienā")]
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        [Description("Nēdēļas nogales dienā")]
        Weekend = Sunday | Saturday,
    }
}