using FishingMap.Domain.Data.Entities;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocationSpecies>()
                .HasKey(ls => new { ls.LocationId, ls.SpeciesId });
            modelBuilder.Entity<LocationSpecies>()
                .HasOne(ls => ls.Location)
                .WithMany(l => l.LocationSpecies)
                .HasForeignKey(ls => ls.LocationId);
            modelBuilder.Entity<LocationSpecies>()
                .HasOne(ls => ls.Species)
                .WithMany(s => s.LocationSpecies)
                .HasForeignKey(ls => ls.SpeciesId);
        }
    }
}