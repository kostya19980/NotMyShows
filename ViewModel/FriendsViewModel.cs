using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class FriendsViewModel
    {
        public List<UserProfile> Friends { get; set; }
    }
}
