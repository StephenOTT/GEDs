using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;
using Plugin;

namespace Data.Mappings
{
    public class ContentFieldMappingMap : EntityTypeConfiguration<ContentFieldMapping>
    {
        public ContentFieldMappingMap()
        {
            this.HasKey(g => g.Id);

            this.Property(g => g.Id)
                .IsRequired();

            this.Property(g => g.Value)
                .IsRequired();

            this.Property(g => g.ValueCode)
                .IsRequired();

            this.Ignore(g => g.State);
        }
    }
}
