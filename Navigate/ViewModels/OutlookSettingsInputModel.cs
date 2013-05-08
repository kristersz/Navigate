using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Navigate.ViewModels
{
    public class OutlookSettingsInputModel
    {
        [DataType(DataType.DateTime)]
        public DateTime IntervalStart { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime IntervalEnd { get; set; }
    }
}