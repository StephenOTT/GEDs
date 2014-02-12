namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewChanges : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Logs", "JobGuid", c => c.String(nullable: false, maxLength: 32));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Logs", "JobGuid", c => c.String(nullable: false));
        }
    }
}
