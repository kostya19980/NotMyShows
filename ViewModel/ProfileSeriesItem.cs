using NotMyShows.Models;
using System;
using System.Collections.Generic;

namespace NotMyShows.ViewModel
{
    public class ProfileSeriesItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public int EpisodesCount { get; set; }
        public int EpisodeTime { get; set; }
        //public int WatchedEpisodesCount { get; set; }
        public SeriesStatus Status { get; set; }
        public string PicturePath { get; set; }
        public int UserRaiting { get; set; }
        public int WatchStatusId { get; set; }
        public DateTime StatusChangedDate { get; set; }
        public DateTime RaitingDate { get; set; }
        public List<UserEpisode> UserEpisodes { get; set; }
    }
    public class UserEpisode
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
