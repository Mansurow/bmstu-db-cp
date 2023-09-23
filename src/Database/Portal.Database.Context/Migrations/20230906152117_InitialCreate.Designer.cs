﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Portal.Database.Context.Roles;

#nullable disable

namespace Portal.Database.Context.Migrations
{
    [DbContext(typeof(AdminDbContext))]
    [Migration("20230906152117_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Portal.Database.Models.BookingDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<int>("AmountPeople")
                        .HasColumnType("integer")
                        .HasColumnName("amount_of_people");

                    b.Property<DateTime>("CreateDateTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("create_date_time");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date")
                        .HasColumnName("date");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone")
                        .HasColumnName("end_time");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("boolean")
                        .HasColumnName("is_paid");

                    b.Property<Guid>("PackageId")
                        .HasColumnType("uuid")
                        .HasColumnName("package_id");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone")
                        .HasColumnName("start_time");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("status");

                    b.Property<double>("TotalPrice")
                        .HasColumnType("numeric")
                        .HasColumnName("total_price");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Guid>("ZoneId")
                        .HasColumnType("uuid")
                        .HasColumnName("zone_id");

                    b.HasKey("Id");

                    b.HasIndex("PackageId");

                    b.HasIndex("UserId");

                    b.HasIndex("ZoneId");

                    b.ToTable("bookings");
                });

            modelBuilder.Entity("Portal.Database.Models.DishDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<double>("Price")
                        .HasColumnType("double precision")
                        .HasColumnName("price");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.ToTable("dishes");
                });

            modelBuilder.Entity("Portal.Database.Models.FeedbackDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date");

                    b.Property<double>("Mark")
                        .HasColumnType("numeric")
                        .HasColumnName("mark");

                    b.Property<string>("Message")
                        .HasColumnType("text")
                        .HasColumnName("message");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid")
                        .HasColumnName("user_id");

                    b.Property<Guid>("ZoneId")
                        .HasColumnType("uuid")
                        .HasColumnName("zone_id");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.HasIndex("ZoneId");

                    b.ToTable("feedbacks");
                });

            modelBuilder.Entity("Portal.Database.Models.InventoryDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<bool>("IsWrittenOff")
                        .HasColumnType("boolean")
                        .HasColumnName("is_written_off");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<DateOnly>("YearOfProduction")
                        .HasColumnType("date")
                        .HasColumnName("date_production");

                    b.Property<Guid>("ZoneId")
                        .HasColumnType("uuid")
                        .HasColumnName("zone_id");

                    b.HasKey("Id");

                    b.HasIndex("ZoneId");

                    b.ToTable("inventories");
                });

            modelBuilder.Entity("Portal.Database.Models.PackageDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<double>("Price")
                        .HasColumnType("numeric")
                        .HasColumnName("price");

                    b.Property<int>("RentalTime")
                        .HasColumnType("integer")
                        .HasColumnName("rental_time");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("type");

                    b.HasKey("Id");

                    b.ToTable("packages");
                });

            modelBuilder.Entity("Portal.Database.Models.UserDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("birthday");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("first_name");

                    b.Property<int>("Gender")
                        .HasColumnType("integer")
                        .HasColumnName("gender");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("last_name");

                    b.Property<string>("MiddleName")
                        .HasColumnType("varchar(64)")
                        .HasColumnName("middle_name");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("password");

                    b.Property<string>("Phone")
                        .HasColumnType("text")
                        .HasColumnName("phone");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("role");

                    b.HasKey("Id");

                    b.ToTable("users");
                });

            modelBuilder.Entity("Portal.Database.Models.ZoneDbModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("address");

                    b.Property<int>("Limit")
                        .HasColumnType("integer")
                        .HasColumnName("limit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("varchar(64)")
                        .HasColumnName("name");

                    b.Property<double>("Rating")
                        .HasColumnType("numeric")
                        .HasColumnName("rating");

                    b.Property<double>("Size")
                        .HasColumnType("double precision")
                        .HasColumnName("size");

                    b.HasKey("Id");

                    b.ToTable("zones");
                });

            modelBuilder.Entity("package_dishes", b =>
                {
                    b.Property<Guid>("dish_id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("package_id")
                        .HasColumnType("uuid");

                    b.HasKey("dish_id", "package_id");

                    b.HasIndex("package_id");

                    b.ToTable("package_dishes");
                });

            modelBuilder.Entity("zone_packages", b =>
                {
                    b.Property<Guid>("package_id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("zone_id")
                        .HasColumnType("uuid");

                    b.HasKey("package_id", "zone_id");

                    b.HasIndex("zone_id");

                    b.ToTable("zone_packages");
                });

            modelBuilder.Entity("Portal.Database.Models.BookingDbModel", b =>
                {
                    b.HasOne("Portal.Database.Models.PackageDbModel", "Package")
                        .WithMany()
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Database.Models.UserDbModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Database.Models.ZoneDbModel", "Zone")
                        .WithMany()
                        .HasForeignKey("ZoneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");

                    b.Navigation("User");

                    b.Navigation("Zone");
                });

            modelBuilder.Entity("Portal.Database.Models.FeedbackDbModel", b =>
                {
                    b.HasOne("Portal.Database.Models.UserDbModel", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Database.Models.ZoneDbModel", "Zone")
                        .WithMany()
                        .HasForeignKey("ZoneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Zone");
                });

            modelBuilder.Entity("Portal.Database.Models.InventoryDbModel", b =>
                {
                    b.HasOne("Portal.Database.Models.ZoneDbModel", "Zone")
                        .WithMany("Inventories")
                        .HasForeignKey("ZoneId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Zone");
                });

            modelBuilder.Entity("package_dishes", b =>
                {
                    b.HasOne("Portal.Database.Models.DishDbModel", null)
                        .WithMany()
                        .HasForeignKey("dish_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Database.Models.PackageDbModel", null)
                        .WithMany()
                        .HasForeignKey("package_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("zone_packages", b =>
                {
                    b.HasOne("Portal.Database.Models.PackageDbModel", null)
                        .WithMany()
                        .HasForeignKey("package_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Portal.Database.Models.ZoneDbModel", null)
                        .WithMany()
                        .HasForeignKey("zone_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Portal.Database.Models.ZoneDbModel", b =>
                {
                    b.Navigation("Inventories");
                });
#pragma warning restore 612, 618
        }
    }
}