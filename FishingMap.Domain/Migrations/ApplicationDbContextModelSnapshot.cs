// <auto-generated />
using System;
using FishingMap.Domain.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;

namespace FishingMap.Domain.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FishingMap.Domain.Data.Entities.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double?>("Area");

                    b.Property<double?>("AverageDepth");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description");

                    b.Property<string>("FishingPermitInfo");

                    b.Property<int?>("LocationOwnerId");

                    b.Property<double?>("MaxDepth");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Polygon>("Points");

                    b.Property<Point>("Position")
                        .IsRequired();

                    b.Property<string>("Rules");

                    b.Property<string>("WebSite");

                    b.HasKey("Id");

                    b.HasIndex("LocationOwnerId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("FishingMap.Domain.Data.Entities.LocationOwner", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("City");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description");

                    b.Property<string>("Email");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Phone");

                    b.Property<string>("StreetAddress");

                    b.Property<string>("WebSite");

                    b.Property<string>("ZipCode");

                    b.HasKey("Id");

                    b.ToTable("LocationOwners");
                });

            modelBuilder.Entity("FishingMap.Domain.Data.Entities.LocationSpecies", b =>
                {
                    b.Property<int>("LocationId");

                    b.Property<int>("SpeciesId");

                    b.HasKey("LocationId", "SpeciesId");

                    b.HasIndex("SpeciesId");

                    b.ToTable("LocationSpecies");
                });

            modelBuilder.Entity("FishingMap.Domain.Data.Entities.Species", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Species");
                });

            modelBuilder.Entity("FishingMap.Domain.Data.Entities.Location", b =>
                {
                    b.HasOne("FishingMap.Domain.Data.Entities.LocationOwner", "Owner")
                        .WithMany("Locations")
                        .HasForeignKey("LocationOwnerId");
                });

            modelBuilder.Entity("FishingMap.Domain.Data.Entities.LocationSpecies", b =>
                {
                    b.HasOne("FishingMap.Domain.Data.Entities.Location", "Location")
                        .WithMany("LocationSpecies")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("FishingMap.Domain.Data.Entities.Species", "Species")
                        .WithMany("LocationSpecies")
                        .HasForeignKey("SpeciesId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
