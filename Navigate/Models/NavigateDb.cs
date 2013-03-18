using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Navigate.Models
{

    public class NavigateDb : DbContext
    {

        public NavigateDb() : base("name=DefaultConnection")
        {
        
        } 

        public DbSet<WorkItem> WorkItems { get; set; }
    }
}