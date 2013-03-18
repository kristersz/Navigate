using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    public class WorkItem
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public string Location { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public decimal EstimatedTime { get; set; }

        public int Priority { get; set; }
        public string AdditionalInfo { get; set; }
    }
}