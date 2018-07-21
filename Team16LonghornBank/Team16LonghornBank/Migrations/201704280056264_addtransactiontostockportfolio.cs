namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addtransactiontostockportfolio : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "StockPortfolio_StockPortfolioID", c => c.Int());
            CreateIndex("dbo.Transactions", "StockPortfolio_StockPortfolioID");
            AddForeignKey("dbo.Transactions", "StockPortfolio_StockPortfolioID", "dbo.StockPortfolios", "StockPortfolioID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "StockPortfolio_StockPortfolioID", "dbo.StockPortfolios");
            DropIndex("dbo.Transactions", new[] { "StockPortfolio_StockPortfolioID" });
            DropColumn("dbo.Transactions", "StockPortfolio_StockPortfolioID");
        }
    }
}
