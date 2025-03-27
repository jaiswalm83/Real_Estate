namespace ODHDEVELOPERS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migra : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DealSheet", "plotcost", c => c.Double(nullable: false));
            AddColumn("dbo.DealSheet", "extra1", c => c.Double(nullable: false));
            AddColumn("dbo.DealSheet", "extra2", c => c.String());
            AddColumn("dbo.DealSheet", "extra3", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DealSheet", "extra3");
            DropColumn("dbo.DealSheet", "extra2");
            DropColumn("dbo.DealSheet", "extra1");
            DropColumn("dbo.DealSheet", "plotcost");
        }
    }
}
