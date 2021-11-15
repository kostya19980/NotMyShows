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
        public int SeriesRaiting { get; set; }
        public int ViewingStatusId { get; set; }
        public ViewingStatus ViewingStatus { get; set; }
    }
    public class ViewingStatus
    {
        public int Id { get; set; }
        public string StatusName { get; set; }
        public List<UserSeries> UserSeries { get; set; }
        public ViewingStatus()
        {
            UserSeries = new List<UserSeries>();
        }
    }
}
