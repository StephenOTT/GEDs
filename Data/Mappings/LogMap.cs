using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;

namespace Data.Mappings
{
    class LogMap : EntityTypeConfiguration<Log>
    {
        public LogMap()
        {
            this.HasKey(i => i.Id);

            this.Property(i => i.JobGuid).IsRequired();
            this.Property(i => i.JobGuid).HasMaxLength(32);
            this.Property(i => i.Type).IsRequired();
            this.Property(i => i.Severity).IsRequired();
            this.Property(i => i.Added).IsRequired();
            this.Property(i => i.Title).IsRequired();
            this.Property(i => i.Message).IsRequired();

            this.Ignore(i => i.State);

        }
    }
}
