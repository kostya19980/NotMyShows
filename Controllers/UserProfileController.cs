using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotMyShows.Models;
using NotMyShows.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NotMyShows.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        readonly SeriesContext db;
        readonly private IHostingEnvironment _env;
        public UserProfileController(SeriesContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }
        public async Task<IActionResult> Profiles()
        {
            List<WatchStatus> watchStatuses = await db.WatchStatuses.ToListAsync();
            UserProfile profile = await GetUserProfile(true);
            List<WatchStatusTab> statusTabs = new List<WatchStatusTab>();
            int Hours = 0;
            foreach (var status in watchStatuses)
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
                        WatchedEpisodesCount = profile.UserEpisodes.Where(x => x.Episode.SeriesId == item.Series.Id).Count(),
                        UserRaiting = item.UserRaiting
                    };
                    Hours += seriesItem.WatchedEpisodesCount * item.Series.EpisodeTime;
                    TabSeriesList.Add(seriesItem);
                }
                WatchStatusTab tab = new WatchStatusTab
                {
                    WatchStatus = status,
                    SeriesList = TabSeriesList
                };
                statusTabs.Add(tab);
            }
            ProfileStats profileStats = new ProfileStats
            {
                EpisodesCount = profile.UserEpisodes.Count(),
                SeriesCount = statusTabs.FirstOrDefault(x=>x.WatchStatus.StatusName == "Просмотрено").SeriesList.Count(),
                HoursSpent = Hours,
                AchievementsCount = 0
            };
            profile.UserSeries = null;
            UserProfileViewModel model = new UserProfileViewModel
            {
                UserProfile = profile,
                StatusTabs = statusTabs,
                ProfileStats = profileStats
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
            int UserRaiting = 0;
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
                UserRaiting = userSeries.UserRaiting;
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
                Episodes = episodes,
                UserRaiting = UserRaiting
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
                        WatchStatus = status,
                        StatusChangedDate = DateTime.Now
                    };
                    userProfile.UserSeries.Add(userSeries);
                    db.Update(userProfile);
                }
                await db.SaveChangesAsync();
            }
            return Json("Success");
        }
        public async Task SelectDefaultWatchStatus(int SeriesId, UserProfile UserProfile, string StatusName)
        {
            UserSeries userSeries = UserProfile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
            WatchStatus status = await db.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == StatusName);
            if (userSeries == null)
            {
                userSeries = new UserSeries
                {
                    SeriesId = SeriesId,
                    WatchStatus = status,
                    StatusChangedDate = DateTime.Now
                };
                UserProfile.UserSeries.Add(userSeries);
            }
            if (userSeries != null && userSeries.WatchStatusId != status.Id)
            {
                userSeries.WatchStatus = status;
            }
            db.Update(UserProfile);
            await db.SaveChangesAsync();
        }
        public async Task IsSeriesWatchCompleted(UserProfile UserProfile, int SeriesId)
        {
            Series series = await db.Series.Include(e => e.Episodes).FirstOrDefaultAsync(x => x.Id == SeriesId);
            int EpisodesCount = series.Episodes.Count();
            int WatchedEpisodesCount = UserProfile.UserEpisodes.Where(x => x.Episode.SeriesId == SeriesId).Count();
            if(EpisodesCount == WatchedEpisodesCount)
            {
                await SelectDefaultWatchStatus(SeriesId, UserProfile, "Просмотрено");
            }
            else
            {
                await SelectDefaultWatchStatus(SeriesId, UserProfile, "Смотрю");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckEpisode(int EpisodeId, int SeriesId)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(us => us.UserSeries)
                .Include(ue => ue.UserEpisodes).ThenInclude(e => e.Episode).FirstOrDefaultAsync(x => x.UserSub == UserSub);
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
            await IsSeriesWatchCompleted(userProfile, SeriesId);
            return Json("Success");
        }
        [HttpPost]
        public async Task<IActionResult> CheckEpisodes(int[] CheckedIds, int SeriesId)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(us => us.UserSeries)
                .Include(ue => ue.UserEpisodes).ThenInclude(e => e.Episode).FirstOrDefaultAsync(x => x.UserSub == UserSub);
            foreach (int id in CheckedIds)
            {
                UserEpisodes userEpisode = userProfile.UserEpisodes.FirstOrDefault(x => x.EpisodeId == id);
                if (userEpisode == null)
                {
                    userEpisode = new UserEpisodes
                    {
                        EpisodeId = id,
                        WatchDate = DateTime.Now
                    };
                    userProfile.UserEpisodes.Add(userEpisode);
                }
            }
            db.Update(userProfile);
            await db.SaveChangesAsync();
            await IsSeriesWatchCompleted(userProfile, SeriesId);
            return Json("Success");
        }
        [HttpPost]
        public async Task<float> SelectSeriesRaiting (int UserRaiting, int SeriesId)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(us => us.UserSeries)
                .ThenInclude(s => s.Series).ThenInclude(r => r.Raiting)
                .FirstOrDefaultAsync(x => x.UserSub == UserSub);
            UserSeries userSeries = userProfile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
            if(userSeries != null)
            {
                int PrevUserRaiting = userSeries.UserRaiting;
                float PrevSeriesRaiting = userSeries.Series.Raiting.Raiting;
                int Votes = userSeries.Series.Raiting.Votes;
                if (PrevUserRaiting == 0)
                {
                    userSeries.Series.Raiting.Raiting = (PrevSeriesRaiting * Votes + UserRaiting) / (Votes + 1);
                    userSeries.Series.Raiting.Votes++;
                }
                else
                {
                    userSeries.Series.Raiting.Raiting = (PrevSeriesRaiting * Votes - PrevUserRaiting + UserRaiting) / Votes;
                }
                userSeries.UserRaiting = UserRaiting;
                userSeries.RaitingDate = DateTime.Now;
                db.Update(userSeries);
                await db.SaveChangesAsync();
                return userSeries.Series.Raiting.Raiting;
            }
            Series series = await db.Series.Include(r => r.Raiting).FirstOrDefaultAsync(x => x.Id == SeriesId);
            return series.Raiting.Raiting;
        }
        //public float CalculateRaiting(float PrevRaiting, int Votes, float UserRaiting)
        //{
        //    return (PrevRaiting * Votes + UserRaiting) / (Votes + 1);
        //}
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
                UserSub = UserSub,
                ImageSrc = Path.Combine("images", "UserAvatars", "DefaultAvatar.png")
            };
            await db.AddAsync(userProfile);
            await db.SaveChangesAsync();
            return RedirectToAction("Profiles");
        }
        public async Task<JsonResult> UploadImage(IFormFile ImageFile)
        {
            if (ImageFile != null)
            {
                string path = Path.Combine("images", "UserAvatars", ImageFile.FileName);
                using (var fileStream = new FileStream(Path.Combine(_env.WebRootPath, path), FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }
                string UserSub = User.GetSub();
                UserProfile profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserSub == UserSub);
                profile.ImageSrc = path;
                db.Update(profile);
                await db.SaveChangesAsync();
            }
            return Json("Success");
        }
    }
}
