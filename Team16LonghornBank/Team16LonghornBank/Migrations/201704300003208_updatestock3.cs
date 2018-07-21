namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatestock3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockPurchases", "TotalStockValue", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockPurchases", "TotalStockValue");
        }
    }
}
