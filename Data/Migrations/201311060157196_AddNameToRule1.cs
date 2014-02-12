namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNameToRule1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.StructureRules", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ComponentRules", "Name", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ComponentRules", "Name", c => c.String());
            AlterColumn("dbo.StructureRules", "Name", c => c.String());
        }
    }
}
