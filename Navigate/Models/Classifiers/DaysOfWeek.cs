using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    [Flags]
    public enum DaysOfWeek
    {
        [Display(Name="Svētdiena")]
        Sunday = 1,

        [Display(Name = "Pirmdiena")]
        Monday = 2,

        [Display(Name = "Otrdiena")]
        Tuesday = 4,

        [Display(Name = "Trešdiena")]
        Wednesday = 8,

        [Display(Name = "Ceturtdiena")]
        Thursday = 16,

        [Display(Name = "Piektdiena")]
        Friday = 32,

        [Display(Name = "Sestdiena")]
        Saturday = 64,
    }
}