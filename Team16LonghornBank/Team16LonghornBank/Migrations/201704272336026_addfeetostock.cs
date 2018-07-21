namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfeetostock : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stocks", "Fee", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stocks", "Fee");
        }
    }
}
