namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addstocks : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StockPortfolios",
                c => new
                    {
                        StockPortfolioID = c.Int(nullable: false, identity: true),
                        AccountNumber = c.Int(nullable: false),
                        AccountName = c.String(),
                        CashValueBalance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        StockPortionValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        isBalanced = c.Boolean(nullable: false),
                        Customer_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.StockPortfolioID)
                .ForeignKey("dbo.AspNetUsers", t => t.Customer_Id)
                .Index(t => t.Customer_Id);
            
            CreateTable(
                "dbo.StockPurchases",
                c => new
                    {
                        StockPurchaseID = c.Int(nullable: false, identity: true),
                        PurchaseDate = c.DateTime(nullable: false),
                        InitialSharePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NumberOfShares = c.Long(nullable: false),
                        Stock_StockID = c.Int(),
                        StockPortfolio_StockPortfolioID = c.Int(),
                    })
                .PrimaryKey(t => t.StockPurchaseID)
                .ForeignKey("dbo.Stocks", t => t.Stock_StockID)
                .ForeignKey("dbo.StockPortfolios", t => t.StockPortfolio_StockPortfolioID)
                .Index(t => t.Stock_StockID)
                .Index(t => t.StockPortfolio_StockPortfolioID);
            
            CreateTable(
                "dbo.Stocks",
                c => new
                    {
                        StockID = c.Int(nullable: false, identity: true),
                        TickerSymbol = c.String(nullable: false),
                        StockName = c.String(nullable: false),
                        StockType = c.String(nullable: false),
                        CurrentPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.StockID);
            
            AddColumn("dbo.Transactions", "StockPortfolioAffected_StockPortfolioID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferFromStockPortfolio_StockPortfolioID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferToStockPortfolio_StockPortfolioID", c => c.Int());
            CreateIndex("dbo.Transactions", "StockPortfolioAffected_StockPortfolioID");
            CreateIndex("dbo.Transactions", "TransferFromStockPortfolio_StockPortfolioID");
            CreateIndex("dbo.Transactions", "TransferToStockPortfolio_StockPortfolioID");
            AddForeignKey("dbo.Transactions", "StockPortfolioAffected_StockPortfolioID", "dbo.StockPortfolios", "StockPortfolioID");
            AddForeignKey("dbo.Transactions", "TransferFromStockPortfolio_StockPortfolioID", "dbo.StockPortfolios", "StockPortfolioID");
            AddForeignKey("dbo.Transactions", "TransferToStockPortfolio_StockPortfolioID", "dbo.StockPortfolios", "StockPortfolioID");
            DropColumn("dbo.AspNetUsers", "SSN");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "SSN", c => c.String());
            DropForeignKey("dbo.StockPortfolios", "Customer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Transactions", "TransferToStockPortfolio_StockPortfolioID", "dbo.StockPortfolios");
            DropForeignKey("dbo.Transactions", "TransferFromStockPortfolio_StockPortfolioID", "dbo.StockPortfolios");
            DropForeignKey("dbo.Transactions", "StockPortfolioAffected_StockPortfolioID", "dbo.StockPortfolios");
            DropForeignKey("dbo.StockPurchases", "StockPortfolio_StockPortfolioID", "dbo.StockPortfolios");
            DropForeignKey("dbo.StockPurchases", "Stock_StockID", "dbo.Stocks");
            DropIndex("dbo.StockPurchases", new[] { "StockPortfolio_StockPortfolioID" });
            DropIndex("dbo.StockPurchases", new[] { "Stock_StockID" });
            DropIndex("dbo.StockPortfolios", new[] { "Customer_Id" });
            DropIndex("dbo.Transactions", new[] { "TransferToStockPortfolio_StockPortfolioID" });
            DropIndex("dbo.Transactions", new[] { "TransferFromStockPortfolio_StockPortfolioID" });
            DropIndex("dbo.Transactions", new[] { "StockPortfolioAffected_StockPortfolioID" });
            DropColumn("dbo.Transactions", "TransferToStockPortfolio_StockPortfolioID");
            DropColumn("dbo.Transactions", "TransferFromStockPortfolio_StockPortfolioID");
            DropColumn("dbo.Transactions", "StockPortfolioAffected_StockPortfolioID");
            DropTable("dbo.Stocks");
            DropTable("dbo.StockPurchases");
            DropTable("dbo.StockPortfolios");
        }
    }
}
