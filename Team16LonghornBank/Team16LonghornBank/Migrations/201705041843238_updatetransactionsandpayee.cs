namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatetransactionsandpayee : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "Payee_PayeeID", c => c.Int());
            CreateIndex("dbo.Transactions", "Payee_PayeeID");
            AddForeignKey("dbo.Transactions", "Payee_PayeeID", "dbo.Payees", "PayeeID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "Payee_PayeeID", "dbo.Payees");
            DropIndex("dbo.Transactions", new[] { "Payee_PayeeID" });
            DropColumn("dbo.Transactions", "Payee_PayeeID");
        }
    }
}
