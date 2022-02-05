using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotMyShows.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string UserSub { get; set; }
        public string ImageSrc { get; set; }
        public List<UserSeries> UserSeries { get; set; }
        public List<UserEpisodes> UserEpisodes { get; set; }
        public UserProfile()
        {
            UserSeries = new List<UserSeries>();
            UserEpisodes = new List<UserEpisodes>();
        }
    }
    public class UserSeries
    {
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
        public int UserRaiting { get; set; }
        public DateTime RaitingDate { get; set; }
        public int WatchStatusId { get; set; }
        public WatchStatus WatchStatus { get; set; }
        public DateTime StatusChangedDate { get; set; }
    }
    public class UserEpisodes
    {
        public int EpisodeId { get; set; }
        public Episode Episode { get; set; }
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
        public DateTime WatchDate { get; set; }
    }
    public class WatchStatus
    {
        public int Id { get; set; }
        public string StatusName { get; set; }
        public List<UserSeries> UserSeries { get; set; }
        public WatchStatus()
        {
            UserSeries = new List<UserSeries>();
        }
    }
}
