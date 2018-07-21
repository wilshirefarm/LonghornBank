namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedtransactionandaddedIRA : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "CheckingAccountAffected_CheckingAccountID", "dbo.CheckingAccounts");
            DropForeignKey("dbo.Transactions", "SavingsAccountAffected_SavingsAccountID", "dbo.SavingsAccounts");
            CreateTable(
                "dbo.IRAccounts",
                c => new
                    {
                        IRAccountID = c.Int(nullable: false, identity: true),
                        AccountNumber = c.Int(nullable: false),
                        AccountName = c.String(),
                        Balance = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxContribution = c.Decimal(nullable: false, precision: 18, scale: 2),
                        isQualified = c.Boolean(nullable: false),
                        Customer_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.IRAccountID)
                .ForeignKey("dbo.AspNetUsers", t => t.Customer_Id)
                .Index(t => t.Customer_Id);
            
            AddColumn("dbo.Transactions", "isBeingDisputed", c => c.Boolean(nullable: false));
            AddColumn("dbo.Transactions", "EmployeeComments", c => c.String());
            AddColumn("dbo.Transactions", "IRAccountAffected_IRAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "SavingsAccount_SavingsAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferFromCheckingAccount_CheckingAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferFromIRAccount_IRAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferFromSavingsAccount_SavingsAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferToCheckingAccount_CheckingAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferToIRAccount_IRAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "TransferToSavingsAccount_SavingsAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "IRAccount_IRAccountID", c => c.Int());
            AddColumn("dbo.Transactions", "CheckingAccount_CheckingAccountID", c => c.Int());
            CreateIndex("dbo.Transactions", "IRAccountAffected_IRAccountID");
            CreateIndex("dbo.Transactions", "SavingsAccount_SavingsAccountID");
            CreateIndex("dbo.Transactions", "TransferFromCheckingAccount_CheckingAccountID");
            CreateIndex("dbo.Transactions", "TransferFromIRAccount_IRAccountID");
            CreateIndex("dbo.Transactions", "TransferFromSavingsAccount_SavingsAccountID");
            CreateIndex("dbo.Transactions", "TransferToCheckingAccount_CheckingAccountID");
            CreateIndex("dbo.Transactions", "TransferToIRAccount_IRAccountID");
            CreateIndex("dbo.Transactions", "TransferToSavingsAccount_SavingsAccountID");
            CreateIndex("dbo.Transactions", "IRAccount_IRAccountID");
            CreateIndex("dbo.Transactions", "CheckingAccount_CheckingAccountID");
            AddForeignKey("dbo.Transactions", "IRAccountAffected_IRAccountID", "dbo.IRAccounts", "IRAccountID");
            AddForeignKey("dbo.Transactions", "TransferFromCheckingAccount_CheckingAccountID", "dbo.CheckingAccounts", "CheckingAccountID");
            AddForeignKey("dbo.Transactions", "TransferFromIRAccount_IRAccountID", "dbo.IRAccounts", "IRAccountID");
            AddForeignKey("dbo.Transactions", "TransferFromSavingsAccount_SavingsAccountID", "dbo.SavingsAccounts", "SavingsAccountID");
            AddForeignKey("dbo.Transactions", "TransferToCheckingAccount_CheckingAccountID", "dbo.CheckingAccounts", "CheckingAccountID");
            AddForeignKey("dbo.Transactions", "TransferToIRAccount_IRAccountID", "dbo.IRAccounts", "IRAccountID");
            AddForeignKey("dbo.Transactions", "TransferToSavingsAccount_SavingsAccountID", "dbo.SavingsAccounts", "SavingsAccountID");
            AddForeignKey("dbo.Transactions", "IRAccount_IRAccountID", "dbo.IRAccounts", "IRAccountID");
            AddForeignKey("dbo.Transactions", "CheckingAccount_CheckingAccountID", "dbo.CheckingAccounts", "CheckingAccountID");
            AddForeignKey("dbo.Transactions", "SavingsAccount_SavingsAccountID", "dbo.SavingsAccounts", "SavingsAccountID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "SavingsAccount_SavingsAccountID", "dbo.SavingsAccounts");
            DropForeignKey("dbo.Transactions", "CheckingAccount_CheckingAccountID", "dbo.CheckingAccounts");
            DropForeignKey("dbo.Transactions", "IRAccount_IRAccountID", "dbo.IRAccounts");
            DropForeignKey("dbo.Transactions", "TransferToSavingsAccount_SavingsAccountID", "dbo.SavingsAccounts");
            DropForeignKey("dbo.Transactions", "TransferToIRAccount_IRAccountID", "dbo.IRAccounts");
            DropForeignKey("dbo.Transactions", "TransferToCheckingAccount_CheckingAccountID", "dbo.CheckingAccounts");
            DropForeignKey("dbo.Transactions", "TransferFromSavingsAccount_SavingsAccountID", "dbo.SavingsAccounts");
            DropForeignKey("dbo.Transactions", "TransferFromIRAccount_IRAccountID", "dbo.IRAccounts");
            DropForeignKey("dbo.Transactions", "TransferFromCheckingAccount_CheckingAccountID", "dbo.CheckingAccounts");
            DropForeignKey("dbo.Transactions", "IRAccountAffected_IRAccountID", "dbo.IRAccounts");
            DropForeignKey("dbo.IRAccounts", "Customer_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Transactions", new[] { "CheckingAccount_CheckingAccountID" });
            DropIndex("dbo.Transactions", new[] { "IRAccount_IRAccountID" });
            DropIndex("dbo.Transactions", new[] { "TransferToSavingsAccount_SavingsAccountID" });
            DropIndex("dbo.Transactions", new[] { "TransferToIRAccount_IRAccountID" });
            DropIndex("dbo.Transactions", new[] { "TransferToCheckingAccount_CheckingAccountID" });
            DropIndex("dbo.Transactions", new[] { "TransferFromSavingsAccount_SavingsAccountID" });
            DropIndex("dbo.Transactions", new[] { "TransferFromIRAccount_IRAccountID" });
            DropIndex("dbo.Transactions", new[] { "TransferFromCheckingAccount_CheckingAccountID" });
            DropIndex("dbo.Transactions", new[] { "SavingsAccount_SavingsAccountID" });
            DropIndex("dbo.Transactions", new[] { "IRAccountAffected_IRAccountID" });
            DropIndex("dbo.IRAccounts", new[] { "Customer_Id" });
            DropColumn("dbo.Transactions", "CheckingAccount_CheckingAccountID");
            DropColumn("dbo.Transactions", "IRAccount_IRAccountID");
            DropColumn("dbo.Transactions", "TransferToSavingsAccount_SavingsAccountID");
            DropColumn("dbo.Transactions", "TransferToIRAccount_IRAccountID");
            DropColumn("dbo.Transactions", "TransferToCheckingAccount_CheckingAccountID");
            DropColumn("dbo.Transactions", "TransferFromSavingsAccount_SavingsAccountID");
            DropColumn("dbo.Transactions", "TransferFromIRAccount_IRAccountID");
            DropColumn("dbo.Transactions", "TransferFromCheckingAccount_CheckingAccountID");
            DropColumn("dbo.Transactions", "SavingsAccount_SavingsAccountID");
            DropColumn("dbo.Transactions", "IRAccountAffected_IRAccountID");
            DropColumn("dbo.Transactions", "EmployeeComments");
            DropColumn("dbo.Transactions", "isBeingDisputed");
            DropTable("dbo.IRAccounts");
            AddForeignKey("dbo.Transactions", "SavingsAccountAffected_SavingsAccountID", "dbo.SavingsAccounts", "SavingsAccountID");
            AddForeignKey("dbo.Transactions", "CheckingAccountAffected_CheckingAccountID", "dbo.CheckingAccounts", "CheckingAccountID");
        }
    }
}
