namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialBuild : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 255),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Structures",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Field = c.String(nullable: false),
                        Source = c.String(nullable: false),
                        Skip = c.Boolean(nullable: false),
                        MaxLength = c.Int(nullable: false),
                        Mandatory = c.Boolean(nullable: false),
                        Default = c.String(),
                        Validation = c.String(),
                        ContentFieldMappingId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContentFieldMappings", t => t.ContentFieldMappingId, cascadeDelete: true)
                .Index(t => t.ContentFieldMappingId);
            
            CreateTable(
                "dbo.ContentFieldMappings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        ValueCode = c.Int(nullable: false),
                        State = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Components",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Field = c.String(nullable: false),
                        Source = c.String(nullable: false),
                        Skip = c.Boolean(nullable: false),
                        MaxLength = c.Int(nullable: false),
                        Mandatory = c.Boolean(nullable: false),
                        Default = c.String(),
                        Validation = c.String(),
                        ContentFieldMappingId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ContentFieldMappings", t => t.ContentFieldMappingId, cascadeDelete: true)
                .Index(t => t.ContentFieldMappingId);
            
            CreateTable(
                "dbo.ComponentOrderPriorities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StructureOrderPriorities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Jobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.Boolean(nullable: false),
                        JobStarted = c.DateTime(nullable: false),
                        JobCompleted = c.DateTime(nullable: false),
                        StructureFileLocation = c.String(),
                        ComponentFileLocation = c.String(),
                        DataSentOn = c.DateTime(nullable: false),
                        SentTo = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RuleActions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(nullable: false),
                        ValueCode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StructureRules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LookupColumnId = c.Int(nullable: false),
                        ActionColumnId = c.Int(nullable: false),
                        RegularExpression = c.String(nullable: false),
                        RuleActionId = c.Int(nullable: false),
                        ReplaceValue = c.String(),
                        ReplaceColumn = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Structures", t => t.LookupColumnId, cascadeDelete: true)
                .ForeignKey("dbo.Structures", t => t.ActionColumnId, cascadeDelete: false)
                .ForeignKey("dbo.RuleActions", t => t.RuleActionId, cascadeDelete: true)
                .Index(t => t.LookupColumnId)
                .Index(t => t.ActionColumnId)
                .Index(t => t.RuleActionId);
            
            CreateTable(
                "dbo.ComponentRules",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LookupColumnId = c.Int(nullable: false),
                        ActionColumnId = c.Int(nullable: false),
                        RegularExpression = c.String(nullable: false),
                        RuleActionId = c.Int(nullable: false),
                        ReplaceValue = c.String(),
                        ReplaceColumn = c.String(),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Components", t => t.LookupColumnId, cascadeDelete: true)
                .ForeignKey("dbo.Components", t => t.ActionColumnId, cascadeDelete: false)
                .ForeignKey("dbo.RuleActions", t => t.RuleActionId, cascadeDelete: true)
                .Index(t => t.LookupColumnId)
                .Index(t => t.ActionColumnId)
                .Index(t => t.RuleActionId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.ComponentRules", new[] { "RuleActionId" });
            DropIndex("dbo.ComponentRules", new[] { "ActionColumnId" });
            DropIndex("dbo.ComponentRules", new[] { "LookupColumnId" });
            DropIndex("dbo.StructureRules", new[] { "RuleActionId" });
            DropIndex("dbo.StructureRules", new[] { "ActionColumnId" });
            DropIndex("dbo.StructureRules", new[] { "LookupColumnId" });
            DropIndex("dbo.Components", new[] { "ContentFieldMappingId" });
            DropIndex("dbo.Structures", new[] { "ContentFieldMappingId" });
            DropForeignKey("dbo.ComponentRules", "RuleActionId", "dbo.RuleActions");
            DropForeignKey("dbo.ComponentRules", "ActionColumnId", "dbo.Components");
            DropForeignKey("dbo.ComponentRules", "LookupColumnId", "dbo.Components");
            DropForeignKey("dbo.StructureRules", "RuleActionId", "dbo.RuleActions");
            DropForeignKey("dbo.StructureRules", "ActionColumnId", "dbo.Structures");
            DropForeignKey("dbo.StructureRules", "LookupColumnId", "dbo.Structures");
            DropForeignKey("dbo.Components", "ContentFieldMappingId", "dbo.ContentFieldMappings");
            DropForeignKey("dbo.Structures", "ContentFieldMappingId", "dbo.ContentFieldMappings");
            DropTable("dbo.ComponentRules");
            DropTable("dbo.StructureRules");
            DropTable("dbo.RuleActions");
            DropTable("dbo.Jobs");
            DropTable("dbo.StructureOrderPriorities");
            DropTable("dbo.ComponentOrderPriorities");
            DropTable("dbo.Components");
            DropTable("dbo.ContentFieldMappings");
            DropTable("dbo.Structures");
            DropTable("dbo.Settings");
        }
    }
}
