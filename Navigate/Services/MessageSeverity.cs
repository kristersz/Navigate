using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Services
{
    public enum MessageSeverity
    {
        /// <summary>
        /// Info. Value 0
        /// </summary>
        [Display(Name = "Info")]
        Info = 0,

        /// <summary>
        /// Brīdinājums. Value 1
        /// </summary>
        [Display(Name = "Brīdinājums")]
        Warning = 1,

        /// <summary>
        /// Kļūda. Value 2
        /// </summary>
        [Display(Name = "Kļūda")]
        Error = 2
    }
}