using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Entities.Models;

namespace Data.Mappings
{
    class JobMap : EntityTypeConfiguration<Job>
    {
        public JobMap()
        {
            this.HasKey(c => c.Id);
            
            this.Property(c => c.Guid).IsRequired();
            this.Property(c => c.Id).IsRequired();
            this.Property(c => c.Status).IsRequired();
            this.Property(c => c.JobStarted).IsRequired();
            this.Property(c => c.JobCompleted).IsRequired();

            this.Ignore(c => c.State);
        }
    }
}
