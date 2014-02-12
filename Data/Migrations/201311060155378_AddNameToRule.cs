namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNameToRule : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StructureRules", "Name", c => c.String());
            AddColumn("dbo.ComponentRules", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ComponentRules", "Name");
            DropColumn("dbo.StructureRules", "Name");
        }
    }
}
