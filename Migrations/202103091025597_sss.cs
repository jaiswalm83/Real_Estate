namespace ODHDEVELOPERS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sss : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DealSheet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        userid = c.String(),
                        customername = c.String(),
                        mobile = c.String(),
                        almobile = c.String(),
                        email = c.String(),
                        address = c.String(),
                        district = c.String(),
                        city = c.String(),
                        state = c.String(),
                        pincode = c.String(),
                        gender = c.String(),
                        occupation = c.String(),
                        introname = c.String(),
                        intromobile = c.String(),
                        intropic = c.String(),
                        unitno = c.String(),
                        unittype = c.String(),
                        unitarea = c.String(),
                        phaseno = c.String(),
                        blockno = c.String(),
                        unitdimension = c.String(),
                        preferenceofunit = c.String(),
                        basesaleprice = c.Int(nullable: false),
                        preloccharge = c.String(),
                        cmembershipcharge = c.String(),
                        pbackupcharge = c.Double(nullable: false),
                        discount = c.String(),
                        totalunitcost = c.String(),
                        dealdays = c.String(),
                        validdate = c.String(),
                        doj = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DealSheet");
        }
    }
}
