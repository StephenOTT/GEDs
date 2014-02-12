using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;

namespace Data.Mappings
{
    public class StructureRuleMap : EntityTypeConfiguration<StructureRule>
    {
        public StructureRuleMap()
        {
            this.HasKey(i => i.Id);

            this.Property(i => i.Name).IsRequired();
            this.Property(i => i.Id).IsRequired();
            this.Property(i => i.LookupColumnId).IsRequired();
            this.Property(i => i.RegularExpression).IsRequired();
            this.Property(i => i.RuleActionId).IsRequired();

            this.Ignore(i => i.State);
        }
    }
}
