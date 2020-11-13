﻿// <auto-generated />
using System;
using ConvAppServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ConvAppServer.Migrations
{
    [DbContext(typeof(SqlContext))]
    [Migration("20201113125318_113")]
    partial class _113
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ConvAppServer.Models.Posting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatorId")
                        .HasColumnType("int");

                    b.Property<bool>("IsRecipe")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("CreatorId");

                    b.ToTable("Postings");
                });

            modelBuilder.Entity("ConvAppServer.Models.PostingNode", b =>
                {
                    b.Property<int>("PostingId")
                        .HasColumnType("int");

                    b.Property<byte>("OrderIndex")
                        .HasColumnType("tinyint");

                    b.Property<string>("ImageFilename")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PostingId", "OrderIndex");

                    b.ToTable("PostingNode");
                });

            modelBuilder.Entity("ConvAppServer.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Category")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Price")
                        .HasColumnType("int");

                    b.Property<byte>("Store")
                        .HasColumnType("tinyint");

                    b.HasKey("Id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("ConvAppServer.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfileImage")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ConvAppServer.Models.UserAuth", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PwdDigest")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("UserAuth");
                });

            modelBuilder.Entity("PostingProduct", b =>
                {
                    b.Property<int>("PostingsId")
                        .HasColumnType("int");

                    b.Property<int>("ProductsId")
                        .HasColumnType("int");

                    b.HasKey("PostingsId", "ProductsId");

                    b.HasIndex("ProductsId");

                    b.ToTable("PostingProduct");
                });

            modelBuilder.Entity("ConvAppServer.Models.Posting", b =>
                {
                    b.HasOne("ConvAppServer.Models.User", null)
                        .WithMany("CreatedPostings")
                        .HasForeignKey("CreatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ConvAppServer.Models.PostingNode", b =>
                {
                    b.HasOne("ConvAppServer.Models.Posting", null)
                        .WithMany("PostingNodes")
                        .HasForeignKey("PostingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ConvAppServer.Models.UserAuth", b =>
                {
                    b.HasOne("ConvAppServer.Models.User", "User")
                        .WithOne()
                        .HasForeignKey("ConvAppServer.Models.UserAuth", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PostingProduct", b =>
                {
                    b.HasOne("ConvAppServer.Models.Posting", null)
                        .WithMany()
                        .HasForeignKey("PostingsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ConvAppServer.Models.Product", null)
                        .WithMany()
                        .HasForeignKey("ProductsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ConvAppServer.Models.Posting", b =>
                {
                    b.Navigation("PostingNodes");
                });

            modelBuilder.Entity("ConvAppServer.Models.User", b =>
                {
                    b.Navigation("CreatedPostings");
                });
#pragma warning restore 612, 618
        }
    }
}
