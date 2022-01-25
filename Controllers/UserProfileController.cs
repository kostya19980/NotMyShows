using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotMyShows.Models;
using NotMyShows.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotMyShows.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        readonly SeriesContext db;
        public UserProfileController(SeriesContext context)
        {
            db = context;
        }
        public async Task<IActionResult> UserProfile()
        {
            List<WatchStatus> watchStatuses = await db.WatchStatuses.ToListAsync();
            UserProfile profile = await GetUserProfile(true);
            List<WatchStatusTab> statusTabs = new List<WatchStatusTab>();
            foreach(var status in watchStatuses)
            {
                List<ProfileSeriesItem> TabSeriesList = new List<ProfileSeriesItem>();
                foreach (var item in profile.UserSeries.Where(x=>x.WatchStatusId == status.Id))
                {
                    ProfileSeriesItem seriesItem = new ProfileSeriesItem
                    {
                        Id = item.Series.Id,
                        Title = item.Series.Title,
                        OriginalTitle = item.Series.OriginalTitle,
                        EpisodesCount = item.Series.Episodes.Count(),
                        Status = new SeriesStatus
                        {
                            Name = StatusColor.GetNewStatusName(item.Series.Status.Name),
                            StatusColorName = StatusColor.GetColor(item.Series.Status.Name)
                        },
                        PicturePath = item.Series.PicturePath,
                        WatchedEpisodesCount = profile.UserEpisodes.Where(x=>x.Episode.SeriesId == item.Series.Id).Count()
                    };
                    TabSeriesList.Add(seriesItem);
                }
                WatchStatusTab tab = new WatchStatusTab
                {
                    WatchStatus = status,
                    SeriesCount = profile.UserSeries.Where(x => x.WatchStatusId == status.Id).Count(),
                    SeriesList = TabSeriesList
                };
                statusTabs.Add(tab);
            }
            //profile.UserSeries = null;
            UserProfileViewModel model = new UserProfileViewModel
            {
                UserProfile = profile,
                StatusTabs = statusTabs
            };
            return View(model);
        }
        public async Task<UserProfile> GetUserProfile(bool IncludeSeries)
        {
            string UserSub = User.GetSub();
            UserProfile profile = new UserProfile();
            if (IncludeSeries)
            {
                profile = await db.UserProfiles
                    .Include(us => us.UserSeries).ThenInclude(s => s.Series).ThenInclude(s => s.Status)
                    .Include(us => us.UserSeries).ThenInclude(s => s.Series).ThenInclude(e => e.Episodes)
                    .Include(ue => ue.UserEpisodes).ThenInclude(e => e.Episode)
                    .FirstOrDefaultAsync(x => x.UserSub == UserSub);
            }
            else
            {
                profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserSub == UserSub);
            }
            return profile;
        }
        public async Task<IActionResult> Series(int SeriesId)
        {
            Series series = await db.Series.Include(r => r.Raiting).Include(s => s.Status).Include(ch => ch.Channel)
                .Include(c => c.Country).Include(sg => sg.SeriesGenres).ThenInclude(g => g.Genre).Include(e => e.Episodes)
                .FirstOrDefaultAsync(x => x.Id == SeriesId);
            UserProfile profile = await GetUserProfile(true);
            var userSeries = profile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
            string CurrentWatchStatus = "";
            List<EpisodeCheckBox> episodes = new List<EpisodeCheckBox>();
            if (userSeries != null)
            {
                var watchStatus = await db.WatchStatuses.FirstOrDefaultAsync(x => x.Id == userSeries.WatchStatusId);
                foreach (var episode in series.Episodes)
                {
                    EpisodeCheckBox episodeCheckBox = new EpisodeCheckBox
                    {
                        Episode = episode,
                        isChecked = profile.UserEpisodes.FirstOrDefault(x => x.EpisodeId == episode.Id) == null ? false : true
                    };
                    episodes.Add(episodeCheckBox);
                }
                CurrentWatchStatus = watchStatus.StatusName;
            }
            else
            {
                foreach (var episode in series.Episodes)
                {
                    EpisodeCheckBox episodeCheckBox = new EpisodeCheckBox
                    {
                        Episode = episode,
                        isChecked = false
                    };
                    episodes.Add(episodeCheckBox);
                }
            }
            episodes.Sort((x, y) => DateTime.Compare(x.Episode.Date ?? DateTime.MaxValue, y.Episode.Date ?? DateTime.MaxValue));
            SeriesView model = new SeriesView
            {
                StatusColorName = StatusColor.GetColor(series.Status.Name),
                Series = series,
                CurrentWatchStatus = CurrentWatchStatus,
                Episodes = episodes
            };
            model.Series.Status.Name = StatusColor.GetNewStatusName(series.Status.Name);
            //if (series == null)
            //{
            //    return StatusCode(StatusCodes.Status404NotFound);
            //}
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SelectWatchStatus(int SeriesId, string StatusName)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(us=>us.UserSeries).FirstOrDefaultAsync(x => x.UserSub == UserSub);
            if (userProfile != null)
            {
                WatchStatus status = await db.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == StatusName);
                UserSeries userSeries = userProfile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
                if (userSeries != null && userSeries.WatchStatusId == status.Id)
                {
                    //userProfile.UserSeries.Remove(userSeries);
                    userSeries.WatchStatus = await db.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == "Брошено");
                }
                if(userSeries != null && userSeries.WatchStatusId != status.Id)
                {
                    userSeries.WatchStatus = status;
                    db.Update(userSeries);
                }
                if(userSeries == null)
                {
                    userSeries = new UserSeries
                    {
                        SeriesId = SeriesId,
                        WatchStatus = status
                    };
                    userProfile.UserSeries.Add(userSeries);
                    db.Update(userProfile);
                }
                await db.SaveChangesAsync();
            }
            return Json(userProfile);
        }
        [HttpPost]
        public async Task<IActionResult> CheckEpisode(int EpisodeId)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(ue => ue.UserEpisodes).FirstOrDefaultAsync(x => x.UserSub == UserSub);
            UserEpisodes userEpisode = userProfile.UserEpisodes.FirstOrDefault(x => x.EpisodeId == EpisodeId);
            if(userEpisode == null)
            {
                userEpisode = new UserEpisodes
                {
                    EpisodeId = EpisodeId,
                    WatchDate = DateTime.Now
                };
                userProfile.UserEpisodes.Add(userEpisode);
            }
            else
            {
                userProfile.UserEpisodes.Remove(userEpisode);
            }
            db.Update(userProfile);
            await db.SaveChangesAsync();
            return Json(userProfile.UserEpisodes);
        }
        [HttpPost]
        public async Task<IActionResult> CheckEpisodes(int[] EpisodesId)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(ue => ue.UserEpisodes).FirstOrDefaultAsync(x => x.UserSub == UserSub);
            foreach(int episodeId in EpisodesId)
            {
                UserEpisodes userEpisode = userProfile.UserEpisodes.FirstOrDefault(x => x.EpisodeId == episodeId);
                if (userEpisode == null)
                {
                    userEpisode = new UserEpisodes
                    {
                        EpisodeId = episodeId,
                        WatchDate = DateTime.Now
                    };
                    userProfile.UserEpisodes.Add(userEpisode);
                }
            }
            db.Update(userProfile);
            await db.SaveChangesAsync();
            return Json(userProfile.UserEpisodes);
        }
        public async Task CreateWatchStatuses()
        {
            string[] names = { "Смотрю", "Запланировано", "Отложено", "Брошено", "Просмотрено" };
            foreach (var name in names)
            {
                WatchStatus status = new WatchStatus
                {
                    StatusName = name
                };
                await db.AddAsync(status);
                await db.SaveChangesAsync();
            }
        }
        public async Task<IActionResult> CreateUserProfile()
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = new UserProfile
            {
                UserSub = UserSub
            };
            await db.AddAsync(userProfile);
            await db.SaveChangesAsync();
            return RedirectToAction("UserProfile");
        }
    }
}
