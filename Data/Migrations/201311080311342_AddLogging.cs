namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLogging : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        JobGuid = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        Severity = c.Int(nullable: false),
                        Added = c.DateTime(nullable: false),
                        Title = c.String(nullable: false),
                        Message = c.String(nullable: false),
                        ClassName = c.String(),
                        MethodName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Jobs", "Guid", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Jobs", "Guid");
            DropTable("dbo.Logs");
        }
    }
}
