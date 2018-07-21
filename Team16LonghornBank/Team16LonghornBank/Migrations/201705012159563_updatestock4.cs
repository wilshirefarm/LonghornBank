namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatestock4 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.StockPortfolios", "FeePortion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StockPortfolios", "FeePortion", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
