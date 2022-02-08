using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class EpisodeViewModel
    {
        public Episode Episode { get; set; }
    }
}
