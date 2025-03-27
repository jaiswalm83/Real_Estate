namespace ODHDEVELOPERS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ads : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DealSheet", "introducerid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DealSheet", "introducerid");
        }
    }
}
