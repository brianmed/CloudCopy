using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

using CloudCopy.Server.Bll;
using CloudCopy.Server.Entities;

namespace CloudCopy.Server.DbContexts
{
    public class CloudCopyDbContext : DbContext
    {
        public DbSet<AdminOptionsEntity> AdminOptions { get; set; }

        public DbSet<AppEntity> App { get; set; }

        public DbSet<CopiedEntity> Copies { get; set; }

        public CloudCopyDbContext()
        {
            ChangeTracker.StateChanged += UpdateTimestamps;
            ChangeTracker.Tracked += UpdateTimestamps;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseLoggerFactory(new Serilog.Extensions.Logging.SerilogLoggerFactory(AppBl.LogSql));

            options
                .UseLazyLoadingProxies()
                .UseSqlite(Program.AppCfg.AppDbConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (IMutableForeignKey fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<AdminOptionsEntity>(entity =>
                entity.HasCheckConstraint("CK_AdminOptions_AdminOptionsEntityId", "AdminOptionsEntityId <= 1 AND AdminOptionsEntityId != 0"));

            modelBuilder.Entity<AppEntity>(entity =>
                entity.HasCheckConstraint("CK_AppEntity_AppEntityId", "AppEntityId <= 1 AND AppEntityId != 0"));
        }

        protected void UpdateTimestamps(object sender, EntityEntryEventArgs e)
        {
            e.Entry.Entity.GetType().GetProperty("Updated").SetValue(e.Entry.Entity, DateTime.UtcNow);
        }        
    }
}