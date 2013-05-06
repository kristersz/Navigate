using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Navigate.Models.Classifiers
{
    public class Category
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<WorkItem> WorkItems { get; set; }
    }
}