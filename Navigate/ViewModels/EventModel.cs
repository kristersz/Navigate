using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.ViewModels
{
    public class EventModel
    {
        public int Id { get; set; }

        public string Subject { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public bool AllDayEvent { get; set; }
    }
}