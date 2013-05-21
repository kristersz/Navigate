using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public enum Reminder
    {
        [Description("Nav nepieciešams atgādināt")]
        None = 0,

        [Description("Došos uz lokāciju ar auto, atgādināt pirms izbraukšanas")]
        Driving = 1,

        [Description("Došos uz lokāciju ar kājām, atgādināt pirms iziešanas")]
        Walking = 2,

        [Description("15 minūtes pirms")]
        FifteenMinutes = 3,

        [Description("30 minūtes pirms")]
        ThirtyMinutes = 4,

        [Description("1 stundu pirms")]
        OneHour = 5,

        [Description("2 stundas pirms")]
        TwoHours = 6,

        [Description("1 dienu pirms")]
        OneDay = 7,

        [Description("2 dienas pirms")]
        TwoDays = 8,

    }
}