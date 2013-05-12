using Navigate.Models.Classifiers;
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

        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<RecurringItem> RecurringItems { get; set; }
        public DbSet<WIRecurrencePattern> WIRecurrencePatterns { get; set; }
        public DbSet<Category> Categories { get; set; }

        //define the necessary relationships
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkItem>()
                .HasOptional(p => p.RecurrencePattern).WithRequired(p => p.WorkItem);
            //modelBuilder.Entity<WorkItem>()
            //    .HasOptional(p => p.RecurringItems);
            modelBuilder.Entity<WorkItem>()
                .HasMany(p => p.Categories).WithMany(p => p.WorkItems)
                .Map(t => t.MapLeftKey("WorkItemId")
                    .MapRightKey("CategoryId")
                    .ToTable("WorkItemCategories"));
        }
    }
}