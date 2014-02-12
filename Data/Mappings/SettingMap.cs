using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;

namespace Data.Mappings
{
    public class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            this.HasKey(s => s.Id);

            //properties
            this.Property(s => s.Id)
                .IsRequired();

            this.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(255);

            this.Property(s => s.Description)
                .HasMaxLength(600);

            this.Property(s => s.Description)
                .HasMaxLength(255);

            this.Ignore(c => c.State);
        }
    }
}
