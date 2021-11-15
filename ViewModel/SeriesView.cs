using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class SeriesView
    {
        public Series Series { get; set; }
        public string StatusColor { get; set; }
        public SeriesView(Series series)
        {
            StatusColor statusColor = new StatusColor();
            StatusColor = statusColor.GetColor(series.Status.Name);
            statusColor.SetNewStatusName(series.Status);
            Series = series;
        }
    }
    public class StatusColor
    {
        public Dictionary<string, StatusDictionary> statusDict = new Dictionary<string, StatusDictionary>();
        public StatusColor()
        {
            statusDict.Add("TBD/On The Bubble", new StatusDictionary { Color = "#F9CC00", StatusName = "PAUSE" });
            statusDict.Add("PAUSE", new StatusDictionary { Color = "#F9CC00", StatusName = "PAUSE" });
            statusDict.Add("Returning Series", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" });
            statusDict.Add("In Development", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" });
            statusDict.Add("ON AIR", new StatusDictionary { Color = "#35BF43", StatusName = "ON AIR" });
            statusDict.Add("Canceled/Ended", new StatusDictionary { Color = "#FF3E1C", StatusName = "DEAD" });
            statusDict.Add("DEAD", new StatusDictionary { Color = "#FF3E1C", StatusName = "DEAD" });
            statusDict.Add("New Series", new StatusDictionary { Color = "var(--color-accent-purple)", StatusName = "NEW" });
            statusDict.Add("NEW", new StatusDictionary { Color = "var(--color-accent-purple)", StatusName = "NEW" });

        }
        public string GetColor(string StatusName)
        {
            return statusDict[StatusName].Color;
        }
        public void SetNewStatusName(Status status)
        {
            status.Name = statusDict[status.Name].StatusName;
        }
    }
    public class StatusDictionary
    {
        public string StatusName { get; set; }
        public string Color { get; set; }
    }
}
