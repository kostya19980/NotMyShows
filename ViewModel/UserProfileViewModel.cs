using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class UserProfileViewModel
    {
        public UserProfile UserProfile { get; set; }
        public List<WatchStatusTab> StatusTabs{ get; set; }
    }
    public class WatchStatusTab
    {
        public int SeriesCount { get; set; }
        public WatchStatus WatchStatus { get; set; }
        public List<ProfileSeriesItem> SeriesList { get; set; }
    }
}
