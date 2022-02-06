﻿// <auto-generated />
using System;
using CarSvc.Dal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarSvc.Dal.Migrations
{
    [DbContext(typeof(CarDbContext))]
    [Migration("20220206192854_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("car_svc")
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CarSvc.Dal.Model.CarEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Alert")
                        .HasColumnType("text")
                        .HasColumnName("alert");

                    b.Property<string>("BodyStyle")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("body_style");

                    b.Property<string>("Brand")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("brand");

                    b.Property<string>("CarId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("car_id");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("color");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_date");

                    b.Property<int>("DoorsCount")
                        .HasColumnType("integer")
                        .HasColumnName("doors_count");

                    b.Property<string>("FuelType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("fuel_type");

                    b.Property<decimal>("LocationLat")
                        .HasColumnType("numeric")
                        .HasColumnName("location_lat");

                    b.Property<decimal>("LocationLon")
                        .HasColumnType("numeric")
                        .HasColumnName("location_lon");

                    b.Property<int>("Mileage")
                        .HasColumnType("integer")
                        .HasColumnName("mileage");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("model");

                    b.Property<DateTime>("ModifiedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("modified_date");

                    b.Property<decimal>("PricePerHour")
                        .HasColumnType("numeric")
                        .HasColumnName("price_per_hour");

                    b.Property<decimal>("PricePerKm")
                        .HasColumnType("numeric")
                        .HasColumnName("price_per_km");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("release_date");

                    b.Property<decimal>("RemainingFuel")
                        .HasColumnType("numeric")
                        .HasColumnName("remaining_fuel");

                    b.Property<string>("Transmission")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("transmission");

                    b.HasKey("Id")
                        .HasName("pk_cars");

                    b.HasIndex("CarId")
                        .IsUnique()
                        .HasDatabaseName("ix_cars_car_id");

                    b.ToTable("cars", "car_svc");
                });

            modelBuilder.Entity("CarSvc.Dal.Model.CarRentEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CarId")
                        .HasColumnType("integer")
                        .HasColumnName("car_id");

                    b.Property<DateTime?>("RentEndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("rent_end_date");

                    b.Property<string>("RentId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("rent_id");

                    b.Property<DateTime>("RentStartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("rent_start_date");

                    b.HasKey("Id")
                        .HasName("pk_car_rents");

                    b.HasIndex("CarId")
                        .HasDatabaseName("ix_car_rents_car_id");

                    b.ToTable("car_rents", "car_svc");
                });

            modelBuilder.Entity("CarSvc.Dal.Model.CarRentEntity", b =>
                {
                    b.HasOne("CarSvc.Dal.Model.CarEntity", "Car")
                        .WithMany("CarRents")
                        .HasForeignKey("CarId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_car_rents_cars_car_id");

                    b.Navigation("Car");
                });

            modelBuilder.Entity("CarSvc.Dal.Model.CarEntity", b =>
                {
                    b.Navigation("CarRents");
                });
#pragma warning restore 612, 618
        }
    }
}
