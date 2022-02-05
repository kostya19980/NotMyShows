﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotMyShows.Models;

namespace NotMyShows.Data.Series
{
    [DbContext(typeof(SeriesContext))]
    [Migration("20220203183624_ProfileUpdate_1")]
    partial class ProfileUpdate_1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.1");

            modelBuilder.Entity("NotMyShows.Models.Channel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Channel");
                });

            modelBuilder.Entity("NotMyShows.Models.Country", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RussianName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Country");
                });

            modelBuilder.Entity("NotMyShows.Models.Episode", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<DateTime?>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("EpisodeNumber")
                        .HasColumnType("int");

                    b.Property<string>("PicturePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SeasonNumber")
                        .HasColumnType("int");

                    b.Property<int>("SeriesId")
                        .HasColumnType("int");

                    b.Property<string>("ShortName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("SeriesId");

                    b.ToTable("Episode");
                });

            modelBuilder.Entity("NotMyShows.Models.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Genre");
                });

            modelBuilder.Entity("NotMyShows.Models.Raitings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<float>("IMDB")
                        .HasColumnType("real");

                    b.Property<int?>("ImdbId")
                        .HasColumnType("int");

                    b.Property<float>("Kinopoisk")
                        .HasColumnType("real");

                    b.Property<int?>("KinopoiskId")
                        .HasColumnType("int");

                    b.Property<float>("Raiting")
                        .HasColumnType("real");

                    b.Property<int?>("SeriesId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SeriesId")
                        .IsUnique()
                        .HasFilter("[SeriesId] IS NOT NULL");

                    b.ToTable("Raitings");
                });

            modelBuilder.Entity("NotMyShows.Models.Review", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Raiting")
                        .HasColumnType("int");

                    b.Property<int>("SeriesId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SeriesId");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("NotMyShows.Models.Series", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<int?>("ChannelId")
                        .HasColumnType("int");

                    b.Property<int?>("CountryId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("EpisodeTime")
                        .HasColumnType("int");

                    b.Property<int>("MyShowsId")
                        .HasColumnType("int");

                    b.Property<string>("OriginalTitle")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PicturePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SeasonCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("StatusId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TotalTime")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Viewers")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ChannelId");

                    b.HasIndex("CountryId");

                    b.HasIndex("StatusId");

                    b.ToTable("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.SeriesGenres", b =>
                {
                    b.Property<int>("SeriesId")
                        .HasColumnType("int");

                    b.Property<int>("GenreId")
                        .HasColumnType("int");

                    b.HasKey("SeriesId", "GenreId");

                    b.HasIndex("GenreId");

                    b.ToTable("SeriesGenres");
                });

            modelBuilder.Entity("NotMyShows.Models.Status", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Status");
                });

            modelBuilder.Entity("NotMyShows.Models.UserEpisodes", b =>
                {
                    b.Property<int>("EpisodeId")
                        .HasColumnType("int");

                    b.Property<int>("UserProfileId")
                        .HasColumnType("int");

                    b.Property<DateTime>("WatchDate")
                        .HasColumnType("datetime2");

                    b.HasKey("EpisodeId", "UserProfileId");

                    b.HasIndex("UserProfileId");

                    b.ToTable("UserEpisodes");
                });

            modelBuilder.Entity("NotMyShows.Models.UserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("ImageSrc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserSub")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("UserProfiles");
                });

            modelBuilder.Entity("NotMyShows.Models.UserSeries", b =>
                {
                    b.Property<int>("SeriesId")
                        .HasColumnType("int");

                    b.Property<int>("UserProfileId")
                        .HasColumnType("int");

                    b.Property<DateTime>("RaitingDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StatusChangedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserRaiting")
                        .HasColumnType("int");

                    b.Property<int>("WatchStatusId")
                        .HasColumnType("int");

                    b.HasKey("SeriesId", "UserProfileId");

                    b.HasIndex("UserProfileId");

                    b.HasIndex("WatchStatusId");

                    b.ToTable("UserSeries");
                });

            modelBuilder.Entity("NotMyShows.Models.WatchStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("StatusName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("WatchStatuses");
                });

            modelBuilder.Entity("NotMyShows.Models.Episode", b =>
                {
                    b.HasOne("NotMyShows.Models.Series", "Series")
                        .WithMany("Episodes")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.Raitings", b =>
                {
                    b.HasOne("NotMyShows.Models.Series", "Series")
                        .WithOne("Raiting")
                        .HasForeignKey("NotMyShows.Models.Raitings", "SeriesId");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.Review", b =>
                {
                    b.HasOne("NotMyShows.Models.Series", "Series")
                        .WithMany("Reviews")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.Series", b =>
                {
                    b.HasOne("NotMyShows.Models.Channel", "Channel")
                        .WithMany("Series")
                        .HasForeignKey("ChannelId");

                    b.HasOne("NotMyShows.Models.Country", "Country")
                        .WithMany("Series")
                        .HasForeignKey("CountryId");

                    b.HasOne("NotMyShows.Models.Status", "Status")
                        .WithMany("Series")
                        .HasForeignKey("StatusId");

                    b.Navigation("Channel");

                    b.Navigation("Country");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("NotMyShows.Models.SeriesGenres", b =>
                {
                    b.HasOne("NotMyShows.Models.Genre", "Genre")
                        .WithMany("SeriesGenres")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotMyShows.Models.Series", "Series")
                        .WithMany("SeriesGenres")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Genre");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.UserEpisodes", b =>
                {
                    b.HasOne("NotMyShows.Models.Episode", "Episode")
                        .WithMany("UserEpisodes")
                        .HasForeignKey("EpisodeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotMyShows.Models.UserProfile", "UserProfile")
                        .WithMany("UserEpisodes")
                        .HasForeignKey("UserProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Episode");

                    b.Navigation("UserProfile");
                });

            modelBuilder.Entity("NotMyShows.Models.UserSeries", b =>
                {
                    b.HasOne("NotMyShows.Models.Series", "Series")
                        .WithMany("UserSeries")
                        .HasForeignKey("SeriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotMyShows.Models.UserProfile", "UserProfile")
                        .WithMany("UserSeries")
                        .HasForeignKey("UserProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NotMyShows.Models.WatchStatus", "WatchStatus")
                        .WithMany("UserSeries")
                        .HasForeignKey("WatchStatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Series");

                    b.Navigation("UserProfile");

                    b.Navigation("WatchStatus");
                });

            modelBuilder.Entity("NotMyShows.Models.Channel", b =>
                {
                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.Country", b =>
                {
                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.Episode", b =>
                {
                    b.Navigation("UserEpisodes");
                });

            modelBuilder.Entity("NotMyShows.Models.Genre", b =>
                {
                    b.Navigation("SeriesGenres");
                });

            modelBuilder.Entity("NotMyShows.Models.Series", b =>
                {
                    b.Navigation("Episodes");

                    b.Navigation("Raiting");

                    b.Navigation("Reviews");

                    b.Navigation("SeriesGenres");

                    b.Navigation("UserSeries");
                });

            modelBuilder.Entity("NotMyShows.Models.Status", b =>
                {
                    b.Navigation("Series");
                });

            modelBuilder.Entity("NotMyShows.Models.UserProfile", b =>
                {
                    b.Navigation("UserEpisodes");

                    b.Navigation("UserSeries");
                });

            modelBuilder.Entity("NotMyShows.Models.WatchStatus", b =>
                {
                    b.Navigation("UserSeries");
                });
#pragma warning restore 612, 618
        }
    }
}
