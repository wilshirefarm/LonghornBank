namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class adddisputeandpayee : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Disputes",
                c => new
                    {
                        DisputeID = c.Int(nullable: false),
                        DisputeComments = c.String(nullable: false),
                        CorrectAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DeleteTransaction = c.Boolean(nullable: false),
                        CurrentStatus = c.String(),
                    })
                .PrimaryKey(t => t.DisputeID)
                .ForeignKey("dbo.Transactions", t => t.DisputeID)
                .Index(t => t.DisputeID);
            
            CreateTable(
                "dbo.Payees",
                c => new
                    {
                        PayeeID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Address = c.String(nullable: false),
                        City = c.String(nullable: false),
                        State = c.String(nullable: false),
                        ZipCode = c.String(nullable: false),
                        PhoneNumber = c.String(nullable: false),
                        PayeeType = c.String(),
                    })
                .PrimaryKey(t => t.PayeeID);
            
            CreateTable(
                "dbo.PayeeAppUsers",
                c => new
                    {
                        Payee_PayeeID = c.Int(nullable: false),
                        AppUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Payee_PayeeID, t.AppUser_Id })
                .ForeignKey("dbo.Payees", t => t.Payee_PayeeID, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.AppUser_Id, cascadeDelete: true)
                .Index(t => t.Payee_PayeeID)
                .Index(t => t.AppUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PayeeAppUsers", "AppUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.PayeeAppUsers", "Payee_PayeeID", "dbo.Payees");
            DropForeignKey("dbo.Disputes", "DisputeID", "dbo.Transactions");
            DropIndex("dbo.PayeeAppUsers", new[] { "AppUser_Id" });
            DropIndex("dbo.PayeeAppUsers", new[] { "Payee_PayeeID" });
            DropIndex("dbo.Disputes", new[] { "DisputeID" });
            DropTable("dbo.PayeeAppUsers");
            DropTable("dbo.Payees");
            DropTable("dbo.Disputes");
        }
    }
}
