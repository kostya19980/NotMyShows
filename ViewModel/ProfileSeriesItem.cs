using System.Collections.Generic;

namespace NotMyShows.ViewModel
{
    public class ProfileSeriesItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public int EpisodesCount { get; set; }
        public int WatchedEpisodesCount { get; set; }
        public SeriesStatus Status { get; set; }
        public string PicturePath { get; set; }
    }
    public class SeriesStatus
    {
        public string Name { get; set; }
        public string StatusColorName { get; set; }
    }
}
