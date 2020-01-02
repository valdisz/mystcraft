﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using atlantis.Persistence;

namespace atlantis.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20200102232959_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0");

            modelBuilder.Entity("atlantis.Persistence.DbGame", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("atlantis.Persistence.DbRegion", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TurnId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("atlantis.Persistence.DbStructure", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TurnId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Structures");
                });

            modelBuilder.Entity("atlantis.Persistence.DbTurn", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Turns");
                });

            modelBuilder.Entity("atlantis.Persistence.DbUnit", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long>("GameId")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Number")
                        .HasColumnType("INTEGER");

                    b.Property<long>("TurnId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Units");
                });
#pragma warning restore 612, 618
        }
    }
}
