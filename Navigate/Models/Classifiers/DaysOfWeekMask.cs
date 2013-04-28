using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    [Flags]
    public enum DaysOfWeekMask
    {
        Sunday = 1,
        
        Monday = 2,

        Tuesday = 4,

        Wednesday = 8,

        Thursday = 16,

        Friday = 32,

        Saturday = 64,

        None = 0,

        All = Sunday | Monday | Tuesday | Wednesday | Thursday | Friday,

        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        Weekend = Sunday | Saturday,
    }
}