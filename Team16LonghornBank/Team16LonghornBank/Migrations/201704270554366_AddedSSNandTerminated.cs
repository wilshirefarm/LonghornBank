namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSSNandTerminated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "SSN", c => c.String());
            AddColumn("dbo.AspNetUsers", "IsNotTerminiated", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsNotTerminiated");
            DropColumn("dbo.AspNetUsers", "SSN");
        }
    }
}
