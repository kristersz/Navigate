using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum Instance
    {
        [Description("Pirmajā")]
        First = 1,

        [Description("Otrajā")]
        Second = 2,

        [Description("Trešajā")]
        Third = 3,

        [Description("Ceturtajā")]
        Fourth = 4,

        [Description("Pēdējā")]
        Last = 5,
    }
}