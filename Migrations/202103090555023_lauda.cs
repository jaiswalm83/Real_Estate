namespace ODHDEVELOPERS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class lauda : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.upload_video_tab",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        file = c.String(),
                        Cdate = c.DateTime(nullable: false),
                        status = c.Int(nullable: false),
                       
                    })
                .PrimaryKey(t => t.Id);

        }
        
        public override void Down()
        {
        }
    }
}
