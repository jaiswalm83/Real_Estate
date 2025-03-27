namespace ODHDEVELOPERS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gfgf : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EMISetDate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        formdate = c.DateTime(nullable: false),
                        branchcode = c.String(),
                        opid = c.String(),
                        status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.EMISetDate");
        }
    }
}
