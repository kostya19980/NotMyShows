using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class SeriesCatalogViewModel
    {
        public IEnumerable<SeriesCatalogItem> SeriesListView { get; set; }
        public List<SeriesFilter> Filters { get; set; }
    }
    public class SeriesCatalogItem
    {
        public int SeriesId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string PicturePath { get; set; }
        public string StatusName { get; set; }
        public string StatusColor { get; set; }
        public float Raiting { get; set; }
        public int? PotentialRating { get; set; }
    }
    public class SeriesFilter
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }
    }
}
