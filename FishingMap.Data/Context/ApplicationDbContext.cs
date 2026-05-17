using FishingMap.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FishingMap.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions options)
            :base(options) { }

        public virtual DbSet<Species> Species { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationOwner> LocationOwners { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Permit> Permits { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<SpeciesRegulation> SpeciesRegulations { get; set; }
        public virtual DbSet<ProtectedPeriod> ProtectedPeriods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.UserName, u.Email })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "RoleUser",
                    u => u.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    r => r.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    ru =>
                    {
                        ru.HasKey("RoleId", "UserId");
                    });

            modelBuilder.Entity<Region>()
                .HasOne(r => r.Parent)
                .WithMany(r => r.Children)
                .HasForeignKey(r => r.ParentRegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Region>()
                .HasIndex(r => r.Type)
                .IsUnique()
                .HasFilter("[Type] = 0");
        }
    }
}
