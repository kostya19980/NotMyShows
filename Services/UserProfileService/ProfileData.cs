using NotMyShows.Models;
using System;
using System.Collections.Generic;

namespace NotMyShows.Services
{
    public class ProfileData
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string ImageSrc { get; set; }
        public IEnumerable<ProfileSeriesItem> UserSeries { get; set; }
        public IEnumerable<Friend> Friends { get; set; }
    }
    public class ProfileSeriesItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public int EpisodesCount { get; set; }
        public int EpisodeTime { get; set; }
        public SeriesStatus Status { get; set; }
        public string PicturePath { get; set; }
        public int UserRaiting { get; set; }
        public int WatchStatusId { get; set; }
        public DateTime StatusChangedDate { get; set; }
        public DateTime RaitingDate { get; set; }
        public IEnumerable<UserEpisodeData> UserEpisodes { get; set; }
    }
    public class UserEpisodeData
    {
        public int EpisodeId { get; set; }
        public string Title { get; set; }
        public DateTime WatchDate { get; set; }
    }
    public class SeriesStatus
    {
        public string Name { get; set; }
        public string StatusColorName { get; set; }
    }
}
