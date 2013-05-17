namespace Navigate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        Email = c.String(),
                        BaseLocation = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.WorkItems",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Subject = c.String(nullable: false, maxLength: 180),
                        Location = c.String(maxLength: 255),
                        Body = c.String(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(nullable: false),
                        AllDayEvent = c.Boolean(nullable: false),
                        EstimatedTime = c.Decimal(precision: 18, scale: 2),
                        WorkItemType = c.Int(nullable: false),
                        Priority = c.Int(),
                        OutlookEntryId = c.String(),
                        isCompleted = c.Boolean(nullable: false),
                        CompletedAt = c.DateTime(),
                        isRecurring = c.Boolean(nullable: false),
                        RecurrenceType = c.Int(),
                        CreatedByUserId = c.Int(nullable: false),
                        UpdatedByUserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfiles", t => t.CreatedByUserId, cascadeDelete: true)
                .Index(t => t.CreatedByUserId);
            
            CreateTable(
                "dbo.RecurrencePatterns",
                c => new
                    {
                        WorkItemId = c.Long(nullable: false),
                        Interval = c.Int(nullable: false),
                        DayOfWeekMask = c.Int(nullable: false),
                        DayOfMonth = c.Int(nullable: false),
                        Instance = c.Int(nullable: false),
                        MonthOfYear = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.WorkItemId)
                .ForeignKey("dbo.WorkItems", t => t.WorkItemId)
                .Index(t => t.WorkItemId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 140),
                        Description = c.String(),
                        CreatedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RecurringItems",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OriginalDate = c.DateTime(nullable: false),
                        WorkItemId = c.Long(nullable: false),
                        Start = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        Subject = c.String(),
                        Location = c.String(),
                        Body = c.String(),
                        isCompleted = c.Boolean(nullable: false),
                        CompletedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkItems", t => t.WorkItemId, cascadeDelete: true)
                .Index(t => t.WorkItemId);
            
            CreateTable(
                "dbo.WorkItemsInCategories",
                c => new
                    {
                        WorkItemId = c.Long(nullable: false),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.WorkItemId, t.CategoryId })
                .ForeignKey("dbo.WorkItems", t => t.WorkItemId, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.WorkItemId)
                .Index(t => t.CategoryId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.WorkItemsInCategories", new[] { "CategoryId" });
            DropIndex("dbo.WorkItemsInCategories", new[] { "WorkItemId" });
            DropIndex("dbo.RecurringItems", new[] { "WorkItemId" });
            DropIndex("dbo.RecurrencePatterns", new[] { "WorkItemId" });
            DropIndex("dbo.WorkItems", new[] { "CreatedByUserId" });
            DropForeignKey("dbo.WorkItemsInCategories", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.WorkItemsInCategories", "WorkItemId", "dbo.WorkItems");
            DropForeignKey("dbo.RecurringItems", "WorkItemId", "dbo.WorkItems");
            DropForeignKey("dbo.RecurrencePatterns", "WorkItemId", "dbo.WorkItems");
            DropForeignKey("dbo.WorkItems", "CreatedByUserId", "dbo.UserProfiles");
            DropTable("dbo.WorkItemsInCategories");
            DropTable("dbo.RecurringItems");
            DropTable("dbo.Categories");
            DropTable("dbo.RecurrencePatterns");
            DropTable("dbo.WorkItems");
            DropTable("dbo.UserProfiles");
        }
    }
}
