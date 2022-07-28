using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class SeriesViewModel
    {
        public Series Series { get; set; }
        public string ReleaseDates { get; set; }
        public int SeasonSize { get; set; }
        public int UserRaiting { get; set; }
        public string StatusColor { get; set; }
        public string CurrentWatchStatus { get; set; }
        public string[] WatchStatuses = new string[] { "Смотрю", "Запланировано", "Отложено", "Брошено", "Просмотрено", "Не смотрю" };
        public List<Season> Seasons { get; set; }
    }
    public class Season
    {
        public int WatchedCount { get; set; }
        public int SeasonNumber { get; set; }
        public List<EpisodeCheckBox> Episodes { get; set; }
    }
    public class EpisodeCheckBox
    {
        public Episode Episode { get; set; }
        public bool IsChecked { get; set; }
    }
    public class StatusColor
    {
        public static Dictionary<string, StatusDictionary> statusDict = new Dictionary<string, StatusDictionary>() {
            { "TBD/On The Bubble", new StatusDictionary { Color = "#F9CC00", StatusName = "PAUSE" } },
            { "PAUSE", new StatusDictionary { Color = "#F9CC00", StatusName = "PAUSE" } },
            { "Returning Series", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" } },
            { "In Development", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" } },
            { "ON AIR", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" } },
            { "Canceled/Ended", new StatusDictionary { Color = "#FF3E1C", StatusName = "DEAD" } },
            { "DEAD", new StatusDictionary { Color = "#FF3E1C", StatusName = "DEAD" } },
            { "New Series", new StatusDictionary { Color = "var(--color-accent-purple)", StatusName = "NEW" } },
            { "NEW", new StatusDictionary { Color = "var(--color-accent-purple)", StatusName = "NEW" } }
        };
        public static string GetColor(string StatusName)
        {
            return statusDict[StatusName].Color;
        }
        public static string GetNewStatusName(string StatusName)
        {
            return statusDict[StatusName].StatusName;
        }
    }
    public class StatusDictionary
    {
        public string StatusName { get; set; }
        public string Color { get; set; }
    }
}
