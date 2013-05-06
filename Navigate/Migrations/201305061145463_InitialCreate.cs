namespace Navigate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.WorkItems",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Subject = c.String(nullable: false, maxLength: 40),
                        Location = c.String(maxLength: 255),
                        StartDateTime = c.DateTime(),
                        EndDateTime = c.DateTime(nullable: false),
                        EstimatedTime = c.Decimal(precision: 18, scale: 2),
                        WorkItemTypeId = c.Int(nullable: false),
                        Priority = c.Int(),
                        Body = c.String(),
                        OutlookEntryId = c.String(),
                        isCompleted = c.Boolean(nullable: false),
                        isRecurring = c.Boolean(nullable: false),
                        RecurrenceType = c.Int(),
                        CreatedByUserId = c.Int(nullable: false),
                        UpdatedByUserId = c.Int(nullable: false),
                        AssignedToUserId = c.Int(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkItemTypes", t => t.WorkItemTypeId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.CreatedByUserId, cascadeDelete: true)
                .Index(t => t.WorkItemTypeId)
                .Index(t => t.CreatedByUserId);
            
            CreateTable(
                "dbo.WorkItemTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false, maxLength: 20),
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
                        IndividualLocation = c.String(),
                        IndividualBody = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.WorkItems", t => t.WorkItemId, cascadeDelete: true)
                .Index(t => t.WorkItemId);
            
            CreateTable(
                "dbo.WIRecurrencePatterns",
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
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.WorkItemCategories",
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
            DropIndex("dbo.WorkItemCategories", new[] { "CategoryId" });
            DropIndex("dbo.WorkItemCategories", new[] { "WorkItemId" });
            DropIndex("dbo.WIRecurrencePatterns", new[] { "WorkItemId" });
            DropIndex("dbo.RecurringItems", new[] { "WorkItemId" });
            DropIndex("dbo.WorkItems", new[] { "CreatedByUserId" });
            DropIndex("dbo.WorkItems", new[] { "WorkItemTypeId" });
            DropForeignKey("dbo.WorkItemCategories", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.WorkItemCategories", "WorkItemId", "dbo.WorkItems");
            DropForeignKey("dbo.WIRecurrencePatterns", "WorkItemId", "dbo.WorkItems");
            DropForeignKey("dbo.RecurringItems", "WorkItemId", "dbo.WorkItems");
            DropForeignKey("dbo.WorkItems", "CreatedByUserId", "dbo.UserProfile");
            DropForeignKey("dbo.WorkItems", "WorkItemTypeId", "dbo.WorkItemTypes");
            DropTable("dbo.WorkItemCategories");
            DropTable("dbo.Categories");
            DropTable("dbo.WIRecurrencePatterns");
            DropTable("dbo.RecurringItems");
            DropTable("dbo.WorkItemTypes");
            DropTable("dbo.WorkItems");
            DropTable("dbo.UserProfile");
        }
    }
}
