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
        [Description("Svētdiena")]
        Sunday = 1,

        [Description("Pirmdiena")]
        Monday = 2,

        [Description("Otrdiena")]
        Tuesday = 4,

        [Description("Trešdiena")]
        Wednesday = 8,

        [Description("Ceturtdiena")]
        Thursday = 16,

        [Description("Piektdiena")]
        Friday = 32,

        [Description("Svētdiena")]
        Saturday = 64,

        [Description("Diena")]
        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday | Saturday,

        [Description("Darbadiena")]
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        [Description("Nēdēļas nogale")]
        Weekend = Sunday | Saturday,
    }
}