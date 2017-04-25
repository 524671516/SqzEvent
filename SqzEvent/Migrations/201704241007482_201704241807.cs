namespace SqzEvent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201704241807 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.VoiceRecord", "duration", c => c.Int(nullable: false, defaultValue:60));
        }
        
        public override void Down()
        {
            DropColumn("dbo.VoiceRecord", "duration");
        }
    }
}
