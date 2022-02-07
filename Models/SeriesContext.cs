using Microsoft.EntityFrameworkCore;

namespace NotMyShows.Models
{

    public class SeriesContext : DbContext
    {
        public DbSet<Series> Series { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Genre> Genre { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<Channel> Channel { get; set; }
        public DbSet<Raitings> Raitings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<WatchStatus> WatchStatuses { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public SeriesContext(DbContextOptions<SeriesContext> options)
            : base(options)
        {
            //Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SeriesGenres>()
                .HasKey(k => new { k.SeriesId, k.GenreId });

            modelBuilder.Entity<SeriesGenres>()
                .HasOne(s => s.Series)
                .WithMany(sg => sg.SeriesGenres)
                .HasForeignKey(s => s.SeriesId);

            modelBuilder.Entity<SeriesGenres>()
                .HasOne(g => g.Genre)
                .WithMany(sg => sg.SeriesGenres)
                .HasForeignKey(g => g.GenreId);
            base.OnModelCreating(modelBuilder);

            //UserSeries
            modelBuilder.Entity<UserSeries>()
                .HasKey(k => new { k.SeriesId, k.UserProfileId });

            modelBuilder.Entity<UserSeries>()
                .HasOne(s => s.Series)
                .WithMany(us => us.UserSeries)
                .HasForeignKey(s => s.SeriesId);

            modelBuilder.Entity<UserSeries>()
                .HasOne(u => u.UserProfile)
                .WithMany(us => us.UserSeries)
                .HasForeignKey(u => u.UserProfileId);
            base.OnModelCreating(modelBuilder);
            //UserEpisodes
            modelBuilder.Entity<UserEpisodes>()
                .HasKey(k => new { k.EpisodeId, k.UserProfileId });

            modelBuilder.Entity<UserEpisodes>()
                .HasOne(e => e.Episode)
                .WithMany(ue => ue.UserEpisodes)
                .HasForeignKey(e => e.EpisodeId);

            modelBuilder.Entity<UserEpisodes>()
                .HasOne(u => u.UserProfile)
                .WithMany(ue => ue.UserEpisodes)
                .HasForeignKey(u => u.UserProfileId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
