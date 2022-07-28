using Microsoft.AspNetCore.Html;
using NotMyShows.Models;
using NotMyShows.Services;
using System;
using System.Collections.Generic;

namespace NotMyShows.ViewModel
{
    public class UserProfileViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string ImageSrc { get; set; }
        public bool IsProfileOwner { get; set; }
        public bool IsFriend { get; set; }
        public ProfileStats ProfileStats { get; set; }

        public List<WatchStatusTab> StatusTabs{ get; set; }
        public IEnumerable<EventGroup> GroupedUserEvents { get; set; }
    }
    public class EventGroup
    {
        public DateTime Date { get; set; }
        public IEnumerable<UserEvent> UserEvents { get; set; }
    }
    public abstract class UserEvent
    {
        public DateTime Date { get; set; }
        public abstract HtmlString GetEventText();
    }
    public class SeriesEvent : UserEvent
    {
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; }
        public string WatchStatus { get; set; }
        public override HtmlString GetEventText()
        {
            string SeriesLink = $"<a href='/Profiles/Series?SeriesId={SeriesId}'>{SeriesTitle}</a>";
            return GetSeriesEventText(SeriesLink);
        }
        public static Dictionary<string, string> watchStatusDict = new Dictionary<string, string>() {
            { "Смотрю", "Смотрит сериал" },
            { "Запланировано", "Запланировал просмотр сериала" },
            { "Отложено", "Отложил просмотр сериала" },
            { "Просмотрено", "Полностью посмотрел сериал" },
            { "Брошено", "Бросил просмотр сериала" }
        };
        public virtual HtmlString GetSeriesEventText(string SeriesLink) 
        {
            string EventText = $"{watchStatusDict[WatchStatus]} {SeriesLink}";
            return new HtmlString(EventText);
        }
    }
    public class EpisodeEvent : SeriesEvent
    {
        public List<UserEpisodeData> EpisodeElements { get; set; }

        public override HtmlString GetSeriesEventText(string SeriesLink)
        {
            string EventText;
            if(EpisodeElements.Count == 1)
            {
                string EpisodeLink = $"<a href='/Profiles/Episode?EpisodeId={EpisodeElements[0].EpisodeId}'>{EpisodeElements[0].Title}</a>";
                EventText = $"Посмотрел эпизод ({EpisodeLink}) сериала {SeriesLink}";
            }
            else
            {
                EventText = $"Посмотрел {EpisodeElements.Count} {DefineEnding(EpisodeElements.Count)} сериала {SeriesLink}";
            }
            return new HtmlString(EventText);
        }
        string DefineEnding(int Count)
        {
            if (((Count % 100) > 10) && ((Count % 100) < 20))
                return "эпизодов";
            if (Count % 10 == 1)
                return "эпизод";
            if ((Count % 10 == 2) || (Count % 10 == 3) || (Count % 10 == 4))
                return "эпизода";
            return "эпизодов";
        }
    }

    public class RaitingEvent : SeriesEvent
    {
        public int Raiting { get; set; }
        public override HtmlString GetSeriesEventText(string SeriesLink)
        {
            string EventText = $"Оценил сериал {SeriesLink} на {Raiting}/10";
            return new HtmlString(EventText);
        }
    }
    public class FriendEvent : UserEvent
    {
        public int UserProfileId { get; set; }
        public string UserName { get; set; }
        public override HtmlString GetEventText()
        {
            string UserLink = $"<a href='/Profiles/Profile?Id={UserProfileId}'>{UserName}</a>";
            string EventText = $"Добавил в друзья пользователя {UserLink}";
            return new HtmlString(EventText);
        }
    }
    public class WatchStatusTab
    {
        public WatchStatus WatchStatus { get; set; }
        public List<ProfileSeriesItem> SeriesList { get; set; }
    }
    public class ProfileStats
    {
        public int EpisodesCount { get; set; }
        public int SeriesCount { get; set; }
        public int HoursSpent { get; set; }
        public int DaysSpent { get; set; }
        public int AchievementsCount { get; set; }
    }
}
