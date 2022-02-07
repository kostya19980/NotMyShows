using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotMyShows.ViewModel
{
    public class EpisodeViewModel
    {
        public Series Series { get; set; }
        public int UserRaiting { get; set; }
        public string StatusColorName { get; set; }
        public string CurrentWatchStatus { get; set; }
        public string[] WatchStatuses = new string[] { "Смотрю", "Запланировано", "Отложено", "Просмотрено" };
        public List<EpisodeCheckBox> Episodes { get; set; }
    }
}
