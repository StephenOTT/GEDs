namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDateSentOnNullableForJobs : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Jobs", "DataSentOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Jobs", "DataSentOn", c => c.DateTime(nullable: false));
        }
    }
}
