namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update2 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.IRAccounts", new[] { "Customer_Id" });
            AlterColumn("dbo.IRAccounts", "Customer_Id", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.IRAccounts", "Customer_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.IRAccounts", new[] { "Customer_Id" });
            AlterColumn("dbo.IRAccounts", "Customer_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.IRAccounts", "Customer_Id");
        }
    }
}
