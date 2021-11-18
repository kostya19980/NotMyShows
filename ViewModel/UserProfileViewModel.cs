using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class UserProfileViewModel
    {
        public UserProfile UserProfile { get; set; }
        public List<ViewingStatusTab> StatusTabs{ get; set; }
    }
    public class ViewingStatusTab
    {
        public int SeriesCount { get; set; }
        public ViewingStatus ViewingStatus { get; set; }
        public List<ProfileSeriesItem> SeriesList { get; set; }
    }
}
