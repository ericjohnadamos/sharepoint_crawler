namespace Insurance.Infrastructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using Insurance.Domain.Entities;

    // Code-Based Configuration and Dependency resolution
    public class InsuranceDbContext : DbContext
    {
        public virtual DbSet<CrawledFiles> CrawledFiles { get; set; }

        public virtual DbSet<ArchivedFiles> ArchivedFiles { get; set; }

        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CrawledFiles>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<ArchivedFiles>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
