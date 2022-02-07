using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace NotMyShows.Models
{
    public class OtherInfoJson
    {
        public int MyShowsId { get; set; }
        public int EpisodeTime { get; set; }
        public string TotalTime { get; set; }
        public Channel Channel { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int[] GenreIds { get; set; }
        public Country Country { get; set; }
        public Raitings Raiting { get; set; }
        public List<Episode> Episodes { get; set; }
    }
    public class Series
    {
        [Key]
        public int Id { get; set; }
        public int MyShowsId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Description { get; set; }
        public int SeasonCount { get; set; }
        public Raitings Raiting { get; set; }
        public int? StatusId { get; set; }
        public Status Status { get; set; }
        public List<SeriesGenres> SeriesGenres { get; set; }
        public List<Episode> Episodes { get; set; }
        public List<Review> Reviews { get; set; }
        public List<Comment> Comments { get; set; }
        public List<UserSeries> UserSeries { get; set; }
        public int EpisodeTime { get; set; }
        public string TotalTime { get; set; }
        public int? ChannelId { get; set; }
        public Channel Channel { get; set; }
        public int? CountryId { get; set; }
        public Country Country { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Viewers { get; set; }
        public string PicturePath { get; set; }
        public Series()
        {
            SeriesGenres = new List<SeriesGenres>();
            UserSeries = new List<UserSeries>();
        }
    }
    public class Episode
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortName { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string PicturePath { get; set; }
        public DateTime? Date { get; set; }
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public List<UserEpisodes> UserEpisodes { get; set; }
        public Episode()
        {
            UserEpisodes = new List<UserEpisodes>();
        }

    }
    public class Status
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Series> Series { get; set; }
    }
    public class Genre
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SeriesGenres> SeriesGenres { get; set; }
        public Genre()
        {
            SeriesGenres = new List<SeriesGenres>();
        }
    }
    public class SeriesGenres
    {
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
    public class Country
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string RussianName { get; set; }
        public List<Series> Series { get; set; }
    }

    public class Channel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Series> Series { get; set; }
    }
    public class Raitings
    {
        public int Id { get; set; }
        public float Raiting { get; set; }
        public int Votes { get; set; }
        public int? KinopoiskId { get; set; }
        public float Kinopoisk { get; set; }
        public int? ImdbId { get; set; }
        public float IMDB { get; set; }
        public int? SeriesId { get; set; }
        public Series Series { get; set; }
    }
    public class Review
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public string Content { get; set; }
        public int Raiting { get; set; }
    }
}