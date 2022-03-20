﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RentSvc.Dal.Model;

#nullable disable

namespace RentSvc.Dal.Migrations
{
    [DbContext(typeof(RentDbContext))]
    partial class RentDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("rent_svc")
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RentSvc.Dal.Model.RentEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CarId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("car_id");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_date");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("data");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("end_date");

                    b.Property<string>("Message")
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<int?>("Mileage")
                        .HasColumnType("integer")
                        .HasColumnName("mileage");

                    b.Property<decimal?>("PricePerHour")
                        .HasColumnType("numeric")
                        .HasColumnName("price_per_hour");

                    b.Property<decimal?>("PricePerKm")
                        .HasColumnType("numeric")
                        .HasColumnName("price_per_km");

                    b.Property<string>("RentId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("rent_id");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("start_date");

                    b.Property<int?>("StartMileage")
                        .HasColumnType("integer")
                        .HasColumnName("start_mileage");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("state");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_rents");

                    b.HasIndex("RentId")
                        .IsUnique()
                        .HasDatabaseName("ix_rents_rent_id");

                    b.ToTable("rents", "rent_svc");
                });

            modelBuilder.Entity("RentSvc.Dal.Model.UserEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DeletedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("deleted_date");

                    b.Property<string>("DriverLicense")
                        .HasColumnType("text")
                        .HasColumnName("driver_license");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("FullName")
                        .HasColumnType("text")
                        .HasColumnName("full_name");

                    b.Property<DateTime>("ModifiedDate")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("modified_date");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text")
                        .HasColumnName("phone_number");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("user_id");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("username");

                    b.Property<bool>("Verified")
                        .HasColumnType("boolean")
                        .HasColumnName("verified");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasDatabaseName("ix_users_user_id");

                    b.ToTable("users", "rent_svc");
                });
#pragma warning restore 612, 618
        }
    }
}