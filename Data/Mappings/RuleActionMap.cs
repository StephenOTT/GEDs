using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;

namespace Data.Mappings
{
    public class RuleActionMap : EntityTypeConfiguration<RuleAction>
    {
        public RuleActionMap()
        {
            this.HasKey(i => i.Id);

            this.Property(i => i.Id).IsRequired();
            this.Property(i => i.Value).IsRequired();
            this.Property(i => i.ValueCode).IsRequired();

            this.Ignore(c => c.State);
        }
    }
}
