namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class editTerminate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsTerminiated", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "IsNotTerminiated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "IsNotTerminiated", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "IsTerminiated");
        }
    }
}
