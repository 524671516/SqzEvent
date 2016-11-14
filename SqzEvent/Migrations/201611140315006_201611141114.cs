namespace SqzEvent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201611141114 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QCAgenda", "CheckinRemark", c => c.String(maxLength: 256));
            AddColumn("dbo.QCAgenda", "CheckoutRemark", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QCAgenda", "CheckoutRemark");
            DropColumn("dbo.QCAgenda", "CheckinRemark");
        }
    }
}
