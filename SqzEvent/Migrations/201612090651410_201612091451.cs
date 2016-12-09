namespace SqzEvent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201612091451 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Off_Recruit", "ApplyTime", c => c.DateTime(nullable: false, defaultValue: new DateTime(2016,12,01)));
            AddColumn("dbo.Off_Recruit", "Off_System_Id", c => c.Int(nullable: false, defaultValue:1));
            CreateIndex("dbo.Off_Recruit", "Off_System_Id");
            AddForeignKey("dbo.Off_Recruit", "Off_System_Id", "dbo.Off_System", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Off_Recruit", "Off_System_Id", "dbo.Off_System");
            DropIndex("dbo.Off_Recruit", new[] { "Off_System_Id" });
            DropColumn("dbo.Off_Recruit", "Off_System_Id");
            DropColumn("dbo.Off_Recruit", "ApplyTime");
        }
    }
}
