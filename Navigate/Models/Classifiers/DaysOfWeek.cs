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
        [Display(Name="Svētdienā")]
        Sunday = 1,

        [Display(Name = "Pirmdienā")]
        Monday = 2,

        [Display(Name = "Otrdienā")]
        Tuesday = 4,

        [Display(Name = "Trešdienā")]
        Wednesday = 8,

        [Display(Name = "Ceturtdienā")]
        Thursday = 16,

        [Display(Name = "Piektdienā")]
        Friday = 32,

        [Display(Name = "Sestdienā")]
        Saturday = 64,
    }
}