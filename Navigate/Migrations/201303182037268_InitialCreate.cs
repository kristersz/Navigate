namespace Navigate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WorkItems",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Subject = c.String(),
                        Location = c.String(),
                        Date = c.DateTime(nullable: false),
                        EstimatedTime = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Priority = c.Int(nullable: false),
                        AdditionalInfo = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WorkItems");
        }
    }
}
