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
        public List<UserSeries> UserSeries { get; set; }
        public UserProfile()
        {
            UserSeries = new List<UserSeries>();
        }
    }
    public class UserSeries
    {
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public int UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }
        public int UserRaiting { get; set; }
        public int WatchStatusId { get; set; }
        public WatchStatus WatchStatus { get; set; }
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
