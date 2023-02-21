using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImmobiliOvunque.Subito
{
    public class IOContext : DbContext
    {
        private string connectionString;

        public DbSet<AnnuncioSubito> Annunci{ get; set; }

        public DbSet<StreamImporter> StreamImporters { get; set; }

        public IOContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                                                                                                                  maxRetryCount: 10,
                                                                                                                  maxRetryDelay: TimeSpan.FromSeconds(30),
                                                                                                                  errorNumbersToAdd: null));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnnuncioSubito>().ToTable("subito_annunci");
            modelBuilder.Entity<StreamImporter>().ToTable("stream_importer");
        }
    }
}
