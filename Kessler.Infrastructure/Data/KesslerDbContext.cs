using Kessler.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kessler.Infrastructure.Data;

public class KesslerDbContext : DbContext
{
    public KesslerDbContext(DbContextOptions<KesslerDbContext> options) : base(options) { }

    public DbSet<OrbitalObject> OrbitalObjects => Set<OrbitalObject>();
    public DbSet<MissionScenario> MissionScenarios => Set<MissionScenario>();
    public DbSet<ReuseMaterialEstimate> ReuseMaterialEstimates => Set<ReuseMaterialEstimate>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Oracle: schema padrão para todos os objetos
        modelBuilder.HasDefaultSchema("RM558652");

        modelBuilder.Entity<OrbitalObject>(e =>
        {
            e.ToTable("ORBITAL_OBJECTS");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).UseIdentityColumn();
            e.Property(o => o.Name).IsRequired().HasMaxLength(200).HasColumnType("NVARCHAR2(200)");
            e.Property(o => o.NoradId).HasMaxLength(20).HasColumnType("VARCHAR2(20)");
            e.Property(o => o.Summary).HasMaxLength(1000).HasColumnType("NVARCHAR2(1000)");
            e.Property(o => o.AltitudeKm).HasColumnType("BINARY_DOUBLE");
            e.Property(o => o.InclinationDeg).HasColumnType("BINARY_DOUBLE");
            e.Property(o => o.EstimatedMassKg).HasColumnType("BINARY_DOUBLE");
            e.Property(o => o.EstimatedSizeM).HasColumnType("BINARY_DOUBLE");
            e.Property(o => o.RegisteredAt).HasColumnType("TIMESTAMP");
            e.Property(o => o.LastUpdatedAt).HasColumnType("TIMESTAMP");

            // Oracle não suporta DELETE CASCADE em FKs com referências circulares — usar Restrict
            e.HasMany(o => o.Missions)
                .WithOne(m => m.OrbitalObject)
                .HasForeignKey(m => m.OrbitalObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(o => o.MaterialEstimates)
                .WithOne(r => r.OrbitalObject)
                .HasForeignKey(r => r.OrbitalObjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MissionScenario>(e =>
        {
            e.ToTable("MISSION_SCENARIOS");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).UseIdentityColumn();
            e.Property(m => m.Name).IsRequired().HasMaxLength(200).HasColumnType("NVARCHAR2(200)");
            e.Property(m => m.Objective).IsRequired().HasMaxLength(500).HasColumnType("NVARCHAR2(500)");
            e.Property(m => m.Summary).HasMaxLength(1000).HasColumnType("NVARCHAR2(1000)");
            e.Property(m => m.EstimatedDeltaVMps).HasColumnType("BINARY_DOUBLE");
            e.Property(m => m.PlannedStartUtc).HasColumnType("TIMESTAMP");
            e.Property(m => m.CompletedAt).HasColumnType("TIMESTAMP");
            e.Property(m => m.RegisteredAt).HasColumnType("TIMESTAMP");
            e.Property(m => m.LastUpdatedAt).HasColumnType("TIMESTAMP");
        });

        modelBuilder.Entity<ReuseMaterialEstimate>(e =>
        {
            e.ToTable("REUSE_MATERIAL_ESTIMATES");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).UseIdentityColumn();
            e.Property(r => r.Name).IsRequired().HasMaxLength(200).HasColumnType("NVARCHAR2(200)");
            e.Property(r => r.Notes).HasMaxLength(1000).HasColumnType("NVARCHAR2(1000)");
            e.Property(r => r.EstimatedSharePct).HasColumnType("BINARY_DOUBLE");
            e.Property(r => r.RegisteredAt).HasColumnType("TIMESTAMP");
            e.Property(r => r.LastUpdatedAt).HasColumnType("TIMESTAMP");
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("USERS");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).UseIdentityColumn();
            e.Property(u => u.Email).IsRequired().HasMaxLength(200).HasColumnType("VARCHAR2(200)");
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Name).IsRequired().HasMaxLength(200).HasColumnType("NVARCHAR2(200)");
            e.Property(u => u.PasswordHash).IsRequired().HasColumnType("VARCHAR2(500)");
            e.Property(u => u.Role).HasMaxLength(50).HasColumnType("VARCHAR2(50)");
            e.Property(u => u.CreatedAt).HasColumnType("TIMESTAMP");
            e.Property(u => u.LastLoginAt).HasColumnType("TIMESTAMP");
        });
    }
}
