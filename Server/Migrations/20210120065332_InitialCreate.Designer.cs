﻿// <auto-generated />
using System;
using CloudCopy.Server.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CloudCopy.Server.Migrations
{
    [DbContext(typeof(CloudCopyDbContext))]
    [Migration("20210120065332_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("CloudCopy.Server.Entities.AdminOptionsEntity", b =>
                {
                    b.Property<long>("AdminOptionsEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Inserted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("SiteUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("AdminOptionsEntityId");

                    b.ToTable("AdminOptions");

                    b.HasCheckConstraint("CK_AdminOptions_AdminOptionsEntityId", "AdminOptionsEntityId <= 1 AND AdminOptionsEntityId != 0");
                });

            modelBuilder.Entity("CloudCopy.Server.Entities.AppEntity", b =>
                {
                    b.Property<long>("AppEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Inserted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("JwtSecret")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PinCode")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("AppEntityId");

                    b.ToTable("App");

                    b.HasCheckConstraint("CK_AppEntity_AppEntityId", "AppEntityId <= 1 AND AppEntityId != 0");
                });

            modelBuilder.Entity("CloudCopy.Server.Entities.CopiedEntity", b =>
                {
                    b.Property<long>("CopiedEntityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Body")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DayCreated")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Inserted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("IpAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Updated")
                        .HasColumnType("TEXT");

                    b.HasKey("CopiedEntityId");

                    b.HasIndex("DayCreated");

                    b.ToTable("Copies");
                });
#pragma warning restore 612, 618
        }
    }
}
