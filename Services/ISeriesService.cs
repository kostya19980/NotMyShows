using Microsoft.EntityFrameworkCore;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotMyShows.Services
{
    public interface ISeriesService
    {
        Task<SeriesCatalogViewModel> GetSeriesListAsync(int startIndex, int count);
        Task<SeriesViewModel> GetSeriesAsync(int SeriesId);
        Task<EpisodeViewModel> GetEpisodeAsync(int EpisodeId);
    }
    public class SeriesService: ISeriesService
    {
        readonly SeriesContext _context;
        private IUserService _userService;
        public SeriesService(SeriesContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }
        public async Task<SeriesCatalogViewModel> GetSeriesListAsync(int startIndex, int count)
        {
            IEnumerable<SeriesCatalogItem> series = _context.Series.Where(x => x.Id >= startIndex && x.Id < startIndex + count)
                .Select(s => new SeriesCatalogItem
                {
                    SeriesId = s.Id,
                    Title = s.Title,
                    OriginalTitle = s.OriginalTitle,
                    PicturePath = s.PicturePath,
                    StatusName = StatusColor.GetNewStatusName(s.Status.Name),
                    StatusColor = StatusColor.GetColor(s.Status.Name),
                    Raiting = s.Raiting.Raiting
                }).AsNoTracking();
            List<SeriesFilter> seriesFilters = new List<SeriesFilter>();

            List<string> filterValues = await _context.Genre.Where(x => x.Name != "").Select(n => n.Name).ToListAsync();
            seriesFilters.Add(new SeriesFilter { Name = "Жанр", Values = filterValues});

            filterValues = await _context.Country.Select(x => x.RussianName).ToListAsync();
            seriesFilters.Add(new SeriesFilter { Name = "Страна", Values = filterValues });

            filterValues = await _context.Channel.Select(x => x.Name).ToListAsync();
            seriesFilters.Add(new SeriesFilter { Name = "Студия", Values = filterValues });

            filterValues = await _context.Status.Select(x => x.Name).ToListAsync();
            seriesFilters.Add(new SeriesFilter { Name = "Статус", Values = filterValues });

            filterValues = new List<string> { "Рейтинг IMDB", "Рейтинг Kinopoisk", "Рейтинг 01Shows" };
            seriesFilters.Add(new SeriesFilter { Name = "Рейтинг", Values = filterValues });

            filterValues = new List<string>();
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 100; i++)
            {
                filterValues.Add((currentYear - i).ToString());
            }
            seriesFilters.Add(new SeriesFilter { Name = "Год выхода", Values = filterValues });

            SeriesCatalogViewModel model = new SeriesCatalogViewModel
            {
                SeriesListView = series,
                Filters = seriesFilters
            };
            return model;
        }
        public async Task<SeriesViewModel> GetSeriesAsync(int SeriesId)
        {
            Series series = await _context.Series.AsNoTracking()
                .Include(r => r.Raiting)
                .Include(s => s.Status)
                .Include(ch => ch.Channel)
                .Include(c => c.Country)
                .Include(sg => sg.SeriesGenres).ThenInclude(g => g.Genre)
                .Include(e => e.Episodes)
                .FirstOrDefaultAsync(x => x.Id == SeriesId);
            string CurrentWatchStatus = "Не смотрю";
            int UserRaiting = 0;
            List<EpisodeCheckBox> episodes = new List<EpisodeCheckBox>();
            if (_userService.IsAuthenticated())
            {
                string UserSub = _userService.GetUserId();
                var profile = await _context.UserProfiles.Select(p => new
                {
                    UserId = p.UserId,
                    UserSeries = p.UserSeries.Select(us => new {
                        SeriesId = us.SeriesId,
                        WatchStatusName = us.WatchStatus.StatusName,
                        UserRaiting = us.UserRaiting
                    }).FirstOrDefault(x => x.SeriesId == SeriesId)
                }).AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserSub);
                if (profile.UserSeries != null)
                {
                    episodes = await CreateEpisodeCheckBoxesAsync(series.Episodes, UserSub, SeriesId);
                    CurrentWatchStatus = profile.UserSeries.WatchStatusName;
                    UserRaiting = profile.UserSeries.UserRaiting;
                }
                else
                {
                    episodes = CreateEpisodeCheckBoxes(series.Episodes);
                }
            }
            else
            {
                episodes = CreateEpisodeCheckBoxes(series.Episodes);
            }
            //episodes.Sort((x, y) => DateTime.Compare(x.Episode.Date ?? DateTime.MaxValue, y.Episode.Date ?? DateTime.MaxValue));

            SeriesViewModel model = new SeriesViewModel
            {
                StatusColor = StatusColor.GetColor(series.Status.Name),
                Series = series,
                ReleaseDates = $"{series.StartDate.ToLongDateString()} - {(series.EndDate == null ? "" : series.EndDate.Value.ToLongDateString())}",
                SeasonSize = series.Episodes.Where(x => x.SeasonNumber == 1 && x.EpisodeNumber != 0).Count(),
                CurrentWatchStatus = CurrentWatchStatus,
                Seasons = CreateSeasons(episodes),
                UserRaiting = UserRaiting
            };
            model.Series.Status.Name = StatusColor.GetNewStatusName(series.Status.Name);
            model.Series.Episodes = null;
            return model;
        }
        public async Task<EpisodeViewModel> GetEpisodeAsync(int EpisodeId)
        {
            Episode episode = await _context.Episodes.Include(c => c.Comments).ThenInclude(p => p.UserProfile).ThenInclude(u => u.User)
                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == EpisodeId);
            EpisodeViewModel model = new EpisodeViewModel
            {
                Episode = episode
            };
            return model;
        }
        public List<Season> CreateSeasons (List<EpisodeCheckBox> episodeCheckBoxes)
        {
            List<Season> seasons = new List<Season>();
            var episodeGroups = episodeCheckBoxes.GroupBy(x => x.Episode.SeasonNumber);
            foreach (var group in episodeGroups)
            {
                Season season = new Season
                {
                    WatchedCount = group.ToList().Where(x => x.IsChecked && x.Episode.EpisodeNumber != 0).Count(),
                    SeasonNumber = group.Key,
                    Episodes = group.ToList()
                };
                seasons.Add(season);
            }
            seasons.Sort((x, y) => x.SeasonNumber.CompareTo(y.SeasonNumber));
            return seasons;
        }
        public List<EpisodeCheckBox> CreateEpisodeCheckBoxes(List<Episode> episodes)
        {
            List<EpisodeCheckBox> checkBoxes = new List<EpisodeCheckBox>();
            foreach (var episode in episodes)
            {
                EpisodeCheckBox episodeCheckBox = new EpisodeCheckBox
                {
                    Episode = episode,
                    IsChecked = false
                };
                checkBoxes.Add(episodeCheckBox);
            }
            checkBoxes.Sort((x, y) => x.Episode.EpisodeNumber.CompareTo(y.Episode.EpisodeNumber));
            return checkBoxes;
        }
        public async Task<List<EpisodeCheckBox>> CreateEpisodeCheckBoxesAsync(List<Episode> episodes, string UserSub, int SeriesId)
        {
            var profile = await _context.UserProfiles.Select(p => new {
                UserId = p.UserId,
                UserEpisodes = p.UserEpisodes.Select(up => new {
                    EpisodeId = up.EpisodeId,
                    SeriesId = up.Episode.SeriesId
                }).Where(x => x.SeriesId == SeriesId)
            }).AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserSub);
            List<EpisodeCheckBox> checkBoxes = new List<EpisodeCheckBox>();
            foreach (var episode in episodes)
            {
                EpisodeCheckBox episodeCheckBox = new EpisodeCheckBox
                {
                    Episode = episode,
                    IsChecked = profile.UserEpisodes.FirstOrDefault(x => x.EpisodeId == episode.Id) == null ? false : true
                };
                checkBoxes.Add(episodeCheckBox);
            }
            checkBoxes.Sort((x, y) => x.Episode.EpisodeNumber.CompareTo(y.Episode.EpisodeNumber));
            return checkBoxes;
        }
    }
}
