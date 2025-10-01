using IntegrationWithExternalParty.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace IntegrationWithExternalParty.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PersonnelRecord> PersonnelRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersonnelRecord>()
            .HasIndex(p => p.PayrollNumber)
            .IsUnique();
    }
}