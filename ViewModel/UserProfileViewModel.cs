using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class UserProfileViewModel
    {
        public UserProfile UserProfile { get; set; }
        //public List<SeriesView> SeriesListView { get; set; }
        public List<StatusViewModel> ViewingStatuses { get; set; }
    }
    public class StatusViewModel
    {
        public int SeriesCount { get; set; }
        public ViewingStatus ViewingStatus { get; set; }  
    }
}
