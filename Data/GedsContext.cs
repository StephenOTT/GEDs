using System.Data.Entity;
using Data.Mappings;

namespace Data
{
    public class GedsContext : DbContext, IDbContext
    {
        static GedsContext()
        {
            Database.SetInitializer<GedsContext>(null);
        }

        public GedsContext()
            : base("Name=GedsContext")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public override int SaveChanges()
        {
            this.ApplyStateChanges();
            return base.SaveChanges();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new SettingMap());
            modelBuilder.Configurations.Add(new StructureMap());
            modelBuilder.Configurations.Add(new ComponentMap());
            modelBuilder.Configurations.Add(new ComponentOrderPriorityMap());
            modelBuilder.Configurations.Add(new StructureOrderPriorityMap());
            modelBuilder.Configurations.Add(new JobMap());
            modelBuilder.Configurations.Add(new RuleActionMap());
            modelBuilder.Configurations.Add(new StructureRuleMap());
            modelBuilder.Configurations.Add(new ComponentRuleMap());
            modelBuilder.Configurations.Add(new LogMap());
        }
    }
}
