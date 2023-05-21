using FishingMap.Domain.Data.Entities;
using FishingMap.Domain.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            :base(options) { }

        public DbSet<Species> Species { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationOwner> LocationOwners { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Permit> Permits { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.UserName, u.Email })
                .IsUnique();

            var passwordSalt = Cryptography.CreateSalt();
            var passwordHash = Cryptography.CreateHash("admin12", passwordSalt);
            var now = DateTime.Now;

            var adminRole = new Role { Id = 1, Name = "Administrator" };
            var userRole = new Role { Id = 2, Name = "User" };

            var adminUser = new User
            {
                Id = 1,
                FirstName = "Lord Admin",
                LastName = "First of His Name",
                Email = "admin@fishingmap.se",
                UserName = "admin",
                Password = passwordHash,
                Salt = passwordSalt,
                Created = now,
                Modified = now
            };

            modelBuilder.Entity<Role>().HasData(adminRole, userRole);
            modelBuilder.Entity<User>().HasData(adminUser);

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
                        ru.HasData(
                            new { RoleId = adminRole.Id, UserId = adminUser.Id },
                            new { RoleId = userRole.Id, UserId = adminUser.Id }
                            );
                    });
        }
    }
}