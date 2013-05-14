using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum Instance
    {
        [Description("First")]
        First = 1,

        [Description("Second")]
        Second = 2,

        [Description("Third")]
        Third = 3,

        [Description("Fourth")]
        Fourth = 4,

        [Description("Last")]
        Last = 5,
    }
}