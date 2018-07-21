namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatestock6 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockPurchases", "StockPurchaseDisplay", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockPurchases", "StockPurchaseDisplay");
        }
    }
}
