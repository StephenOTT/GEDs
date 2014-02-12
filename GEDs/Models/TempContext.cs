using System;
using System.Data.Entity;

using Entities.Models;

namespace GEDs.Models
{
    public class TempContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
        }

    }
}