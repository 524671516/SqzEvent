namespace SqzEvent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201701041427 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Off_WeekendBreak",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StoreManagerId = c.Int(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                        Subscribe = c.DateTime(nullable: false),
                        SignInTime = c.DateTime(nullable: false),
                        UserName = c.String(maxLength: 32),
                        LastUploadTime = c.DateTime(),
                        TrailDefault = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Off_StoreManager", t => t.StoreManagerId)
                .ForeignKey("dbo.Off_Checkin_Schedule", t => t.ScheduleId)
                .Index(t => t.StoreManagerId)
                .Index(t => t.ScheduleId);
            
            CreateTable(
                "dbo.Off_WeekendBreakRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WeekendBreakId = c.Int(nullable: false),
                        UploadTime = c.DateTime(nullable: false),
                        SalesCount = c.Int(nullable: false),
                        TrailCount = c.Int(nullable: false),
                        SalesDetails = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Off_WeekendBreak", t => t.WeekendBreakId, cascadeDelete: true)
                .Index(t => t.WeekendBreakId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Off_WeekendBreak", "ScheduleId", "dbo.Off_Checkin_Schedule");
            DropForeignKey("dbo.Off_WeekendBreak", "StoreManagerId", "dbo.Off_StoreManager");
            DropForeignKey("dbo.Off_WeekendBreakRecord", "WeekendBreakId", "dbo.Off_WeekendBreak");
            DropIndex("dbo.Off_WeekendBreakRecord", new[] { "WeekendBreakId" });
            DropIndex("dbo.Off_WeekendBreak", new[] { "ScheduleId" });
            DropIndex("dbo.Off_WeekendBreak", new[] { "StoreManagerId" });
            DropTable("dbo.Off_WeekendBreakRecord");
            DropTable("dbo.Off_WeekendBreak");
        }
    }
}
