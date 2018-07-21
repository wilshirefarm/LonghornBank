namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatestock5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockPurchases", "ChangeInPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.StockPurchases", "TotalChange", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockPurchases", "TotalChange");
            DropColumn("dbo.StockPurchases", "ChangeInPrice");
        }
    }
}
