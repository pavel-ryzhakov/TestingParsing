﻿// <auto-generated />
using ConsoleParser;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ConsoleParser.Migrations
{
    [DbContext(typeof(PassportDbContext))]
    partial class PassportDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ConsoleParser.Passport", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("PASSP_NUMBER")
                        .HasColumnType("integer")
                        .HasColumnName("passp_number");

                    b.Property<int>("PASSP_SERIES")
                        .HasColumnType("integer")
                        .HasColumnName("passp_series");

                    b.HasKey("Id");

                    b.ToTable("passports", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
