using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class SeriesViewModel
    {
        public List<SeriesView> SeriesListView { get; set; }
    }
}
