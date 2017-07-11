namespace SqzEvent.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201707111202 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductClass",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductClassName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            Sql("insert into dbo.ProductClass (ProductClassName) values('ºìÌÇ½ª²è')");
            AddColumn("dbo.Product", "ProductClassId", c => c.Int(nullable: false, defaultValue: 1));
            CreateIndex("dbo.Product", "ProductClassId");
            AddForeignKey("dbo.Product", "ProductClassId", "dbo.ProductClass", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Product", "ProductClassId", "dbo.ProductClass");
            DropIndex("dbo.Product", new[] { "ProductClassId" });
            DropColumn("dbo.Product", "ProductClassId");
            DropTable("dbo.ProductClass");
        }
    }
}
