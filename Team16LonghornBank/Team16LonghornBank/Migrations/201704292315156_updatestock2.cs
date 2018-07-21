namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatestock2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StockPortfolios", "FeePortion", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.StockPortfolios", "isPending", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StockPortfolios", "isPending");
            DropColumn("dbo.StockPortfolios", "FeePortion");
        }
    }
}
