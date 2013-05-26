using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.ViewModels
{
    public class EventModel
    {
        public int Id { get; set; }

        public int dayDelta { get; set; }

        public int minuteDelta { get; set; }

        public bool AllDayEvent { get; set; }
    }
}