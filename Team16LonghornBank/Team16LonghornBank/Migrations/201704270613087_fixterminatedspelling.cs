namespace Team16LonghornBank.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixterminatedspelling : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsTerminated", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "IsTerminiated");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "IsTerminiated", c => c.Boolean(nullable: false));
            DropColumn("dbo.AspNetUsers", "IsTerminated");
        }
    }
}
