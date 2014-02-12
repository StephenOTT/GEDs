using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;

namespace Data.Mappings
{
    class ComponentMap : EntityTypeConfiguration<Component>
    {
        public ComponentMap()
        {
            this.HasKey(c => c.Id);

            this.Property(c => c.Id).IsRequired();
            this.Property(c => c.Field).IsRequired();
            this.Property(c => c.Order).IsRequired();
            this.Property(c => c.Skip).IsRequired();
            this.Property(c => c.Source).IsRequired();

            this.Ignore(c => c.State);

            this.ToTable("Components");

            this.HasRequired(c => c.ContentFieldMapping);
        }
    }
}
