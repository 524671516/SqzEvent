namespace SqzEvent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201611281829 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AgendaTemplate", "Unit", c => c.String(maxLength: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AgendaTemplate", "Unit");
        }
    }
}
