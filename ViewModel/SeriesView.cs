using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class SeriesView
    {
        public Series Series { get; set; }
        public string StatusColorName { get; set; }
        public string CurrentViewingStatus { get; set; }
        public string[] ViewingStatuses = new string[] { "Смотрю", "Запланировано", "Отложено" };
        public SeriesView(Series series)
        {
            StatusColorName = StatusColor.GetColor(series.Status.Name);
            series.Status.Name = StatusColor.GetNewStatusName(series.Status.Name);
            Series = series;
        }
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
        //public StatusColor()
        //{
        //    statusDict.Add("TBD/On The Bubble", new StatusDictionary { Color = "#F9CC00", StatusName = "PAUSE" });
        //    statusDict.Add("PAUSE", new StatusDictionary { Color = "#F9CC00", StatusName = "PAUSE" });
        //    statusDict.Add("Returning Series", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" });
        //    statusDict.Add("In Development", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" });
        //    statusDict.Add("ON AIR", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" });
        //    statusDict.Add("Canceled/Ended", new StatusDictionary { Color = "#FF3E1C", StatusName = "DEAD" });
        //    statusDict.Add("DEAD", new StatusDictionary { Color = "#FF3E1C", StatusName = "DEAD" });
        //    statusDict.Add("New Series", new StatusDictionary { Color = "var(--color-accent-purple)", StatusName = "NEW" });
        //    statusDict.Add("NEW", new StatusDictionary { Color = "var(--color-accent-purple)", StatusName = "NEW" });

        //}
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
