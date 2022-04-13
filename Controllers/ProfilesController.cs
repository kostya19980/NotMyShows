using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NotMyShows.Controllers
{
    [Authorize]
    public class ProfilesController : Controller
    {
        readonly SeriesContext db;
        readonly private IWebHostEnvironment _env;
        public ProfilesController(SeriesContext context, IWebHostEnvironment env)
        {
            db = context;
            _env = env;
        }
        public async Task<IActionResult> Profile(int? Id)
        {
            if (Id == null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    Id = await GetUserProfileId();
                }
            }
            List<WatchStatus> watchStatuses = await db.WatchStatuses.AsNoTracking().ToListAsync();
            var profile = await db.UserProfiles.Select(p => new 
            {
                Id = p.Id,
                Name = p.User.UserName,
                ImageSrc = p.ImageSrc,
                UserSeries = p.UserSeries.Select(us => new ProfileSeriesItem
                {
                    Id = us.SeriesId,
                    Title = us.Series.Title,
                    OriginalTitle = us.Series.OriginalTitle,
                    EpisodesCount = us.Series.Episodes.Count(),
                    EpisodeTime = us.Series.EpisodeTime,
                    WatchStatusId = us.WatchStatusId,
                    Status = new SeriesStatus
                    {
                        Name = StatusColor.GetNewStatusName(us.Series.Status.Name),
                        StatusColorName = StatusColor.GetColor(us.Series.Status.Name)
                    },
                    PicturePath = us.Series.PicturePath,
                    //WatchedEpisodesCount = p.UserEpisodes.Where(x => x.Episode.SeriesId == us.Series.Id).Count(),
                    UserRaiting = us.UserRaiting,
                    StatusChangedDate = us.StatusChangedDate,
                    RaitingDate = us.RaitingDate,
                    UserEpisodes = us.UserProfile.UserEpisodes
                    .Where(x => x.Episode.SeriesId == us.Series.Id)
                    .Select(x => new UserEpisode
                    {
                        EpisodeId = x.EpisodeId,
                        Title = x.Episode.ShortName,
                        WatchDate = x.WatchDate
                    }).ToList()
                }),
                Friends = p.Friends.Select(uf => new Friend
                {
                    FriendProfileId = uf.FriendProfileId,
                    Date = uf.Date
                })
            }).AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
            var seriesGroups = profile.UserSeries.GroupBy(s => s.WatchStatusId);
            List<WatchStatusTab> statusTabs = new List<WatchStatusTab>();
            foreach (var status in watchStatuses)
            {
                List<ProfileSeriesItem> TabSeriesList = new List<ProfileSeriesItem>();
                if (seriesGroups.Any())
                {
                    var tempSeriesList = seriesGroups.FirstOrDefault(x => x.Key == status.Id);
                    if (tempSeriesList != null)
                    {
                        TabSeriesList = tempSeriesList.ToList();
                    }
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
                EpisodesCount = profile.UserSeries.Sum(x => x.UserEpisodes.Count()),
                SeriesCount = statusTabs.FirstOrDefault(x=>x.WatchStatus.StatusName == "Просмотрено").SeriesList.Count(),
                HoursSpent = profile.UserSeries.Sum(x => x.UserEpisodes.Count() * x.EpisodeTime)/60,
                AchievementsCount = 0
            };
            bool isFriend = false;
            if (User.Identity.IsAuthenticated)
            {
                int UserProfileId = await GetUserProfileId();
                if(UserProfileId != profile.Id)
                {
                    UserProfile userProfile = await db.UserProfiles.Include(f => f.Friends).FirstOrDefaultAsync(x => x.Id == UserProfileId);
                    isFriend = userProfile.Friends.FirstOrDefault(x => x.FriendProfileId == profile.Id) == null ? false : true;
                }
            }
            UserProfileViewModel model = new UserProfileViewModel
            {
                Id = profile.Id,
                UserName = profile.Name,
                ImageSrc = profile.ImageSrc,
                IsFriend = isFriend,
                StatusTabs = statusTabs,
                ProfileStats = profileStats,
                UserEvents = await CreateUserEventsAsync(profile.UserSeries, profile.Friends, watchStatuses)
            };
            return View("UserProfile", model);
        }
        public async Task<List<UserEvent>> CreateUserEventsAsync(IEnumerable<ProfileSeriesItem> SeriesItems, IEnumerable<Friend> Friends, List<WatchStatus> watchStatuses)
        {
            /////////User Events
            List<UserEvent> userEvents = new List<UserEvent>();
            foreach (var series in SeriesItems)
            {
                UserEvent seriesEvent = new SeriesEvent
                {
                    SeriesId = series.Id,
                    SeriesTitle = series.Title,
                    Date = series.StatusChangedDate,
                    WatchStatus = watchStatuses.FirstOrDefault(x => x.Id == series.WatchStatusId).StatusName
                };
                userEvents.Add(seriesEvent);
                if (series.UserRaiting > 0)
                {
                    UserEvent raitingEvent = new RaitingEvent
                    {
                        SeriesId = series.Id,
                        SeriesTitle = series.Title,
                        Date = series.RaitingDate,
                        Raiting = series.UserRaiting
                    };
                    userEvents.Add(raitingEvent);
                }
                var groupedEpisodes = series.UserEpisodes
                    .GroupBy(i => i.WatchDate.Year, (k, g) => g
                    .GroupBy(i => (long)(i.WatchDate - g.Min(e => e.WatchDate)).TotalMinutes / 5))
                    .SelectMany(g => g);
                foreach (var group in groupedEpisodes)
                {
                    List<UserEpisode> episodesList = group.ToList();
                    UserEvent episodeEvent = new EpisodeEvent
                    {
                        SeriesId = series.Id,
                        SeriesTitle = series.Title,
                        EpisodeElements = episodesList,
                        Date = episodesList.Max(x => x.WatchDate),
                    };
                    userEvents.Add(episodeEvent);
                }
            }
            foreach (var friend in Friends)
            {
                UserEvent friendEvent = new FriendEvent
                {
                    UserName = await GetUserNameById(friend.FriendProfileId),
                    UserProfileId = friend.FriendProfileId,
                    Date = friend.Date
                };
                userEvents.Add(friendEvent);
            }
            userEvents.Sort((x, y) => x.Date.CompareTo(y.Date));
            userEvents.Reverse();
            return userEvents;
        }
        public async Task<string> GetUserNameById(int Id)
        {
            var profile = await db.UserProfiles.Select(p => new { Id = p.Id, Name = p.User.UserName }).AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
            return profile.Name;
        }
        public async Task<int> GetUserProfileId()
        {
            string UserSub = User.GetSub();
            UserProfile profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == UserSub);
            return profile.Id;
        }
        //public async Task<UserProfile> GetUserProfile(int Id, bool IncludeSeries)
        //{
        //    //string UserSub = User.GetSub();
        //    UserProfile profile = new UserProfile();
        //    if (IncludeSeries)
        //    {
        //        profile = await db.UserProfiles
        //            .Include(us => us.UserSeries).ThenInclude(s => s.Series).ThenInclude(s => s.Status)
        //            .Include(us => us.UserSeries).ThenInclude(s => s.Series).ThenInclude(e => e.Episodes)
        //            .Include(ue => ue.UserEpisodes).ThenInclude(e => e.Episode)
        //            .FirstOrDefaultAsync(x => x.Id == Id);
        //    }
        //    else
        //    {
        //        profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == Id);
        //    }
        //    return profile;
        //}
        public async Task<IActionResult> Episode(int EpisodeId)
        {
            Episode episode = await db.Episodes.Include(c => c.Comments).ThenInclude(p => p.UserProfile).ThenInclude(u => u.User)
                .AsNoTracking().FirstOrDefaultAsync(x => x.Id == EpisodeId);
            EpisodeViewModel model = new EpisodeViewModel
            {
                Episode = episode
            };
            return View(model);
        }
        public async Task<IActionResult> Friends()
        {
            string UserSub = User.GetSub();
            var profile = await db.UserProfiles.Include(f => f.Friends)
                .AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserSub);
            List<int> profileIds = new List<int>();
            foreach(var item in profile.Friends)
            {
                profileIds.Add(item.FriendProfileId);
            }
            List<UserProfile> friendProfiles = await db.UserProfiles.Include(u => u.User).Where(x => profileIds.Contains(x.Id)).AsNoTracking().ToListAsync();
            FriendsViewModel model = new FriendsViewModel
            {
                Friends = friendProfiles
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(string CommentText, int EpisodeId)
        {
            string UserSub = User.GetSub();
            UserProfile profile = await db.UserProfiles.Include(c => c.Comments).FirstOrDefaultAsync(x => x.UserId == UserSub);
            Comment comment = new Comment
            {
                EpisodeId = EpisodeId,
                Text = CommentText,
                Likes = 0,
                Dislikes = 0,
                Date = DateTime.Now
            };
            profile.Comments.Add(comment);
            db.Update(profile);
            await db.SaveChangesAsync();
            return Json("Success");
        }
        [HttpPost]
        public async Task<IActionResult> AddFriend(int FriendId)
        {
            string UserSub = User.GetSub();
            UserProfile profile = await db.UserProfiles.Include(f => f.Friends).FirstOrDefaultAsync(x => x.UserId == UserSub);
            if (profile.Friends.FirstOrDefault(x => x.FriendProfileId == FriendId) == null)
            {
                Friend friend = new Friend
                {
                    FriendProfileId = FriendId,
                    Date = DateTime.Now
                };
                profile.Friends.Add(friend);
                db.Update(profile);
                await db.SaveChangesAsync();
                return Json("Success");
            }
            return Json("Пользователь уже находится у вас в друзьях!");
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFriend(int FriendId)
        {
            string UserSub = User.GetSub();
            UserProfile profile = await db.UserProfiles.Include(f => f.Friends).FirstOrDefaultAsync(x => x.UserId == UserSub);
            Friend friend = profile.Friends.FirstOrDefault(x => x.FriendProfileId == FriendId);
            if (friend != null)
            {
                profile.Friends.Remove(friend);
                db.Update(profile);
                await db.SaveChangesAsync();
                return Json("Success");
            }
            return Json("Пользователя нет в друзьях!");
        }
        public async Task<IActionResult> Series(int SeriesId)
        {
            Series series = await db.Series.Include(r => r.Raiting).Include(s => s.Status).Include(ch => ch.Channel)
                .Include(c => c.Country).Include(sg => sg.SeriesGenres).ThenInclude(g => g.Genre).Include(e => e.Episodes)
                .FirstOrDefaultAsync(x => x.Id == SeriesId);
            string CurrentWatchStatus = "";
            int UserRaiting = 0;
            List<EpisodeCheckBox> episodes = new List<EpisodeCheckBox>();
            if (User.Identity.IsAuthenticated)
            {
                string UserSub = User.GetSub();
                //int id = db.UserProfiles.FirstOrDefaultAsync(x => x.UserSub == UserSub).Result.Id;
                UserProfile profile = await db.UserProfiles.Include(us => us.UserSeries).Include(ue => ue.UserEpisodes).FirstOrDefaultAsync(x => x.UserId == UserSub);
                var userSeries = profile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
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
                    episodes = DefaultEpisodeCheckBoxes(series.Episodes);
                }
            }
            else
            {
                episodes = DefaultEpisodeCheckBoxes(series.Episodes);
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
        public List<EpisodeCheckBox> DefaultEpisodeCheckBoxes(List<Episode> episodes)
        {
            List<EpisodeCheckBox> checkBoxes = new List<EpisodeCheckBox>();
            foreach (var episode in episodes)
            {
                EpisodeCheckBox episodeCheckBox = new EpisodeCheckBox
                {
                    Episode = episode,
                    isChecked = false
                };
                checkBoxes.Add(episodeCheckBox);
            }
            return checkBoxes;
        }
        [HttpPost]
        public async Task<IActionResult> SelectWatchStatus(int SeriesId, string StatusName)
        {
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.Include(us=>us.UserSeries).FirstOrDefaultAsync(x => x.UserId == UserSub);
            if (userProfile != null)
            {
                WatchStatus status = await db.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == StatusName);
                UserSeries userSeries = userProfile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
                if (userSeries != null && userSeries.WatchStatusId == status.Id)
                {
                    //userProfile.UserSeries.Remove(userSeries);
                    userSeries.WatchStatus = await db.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == "Брошено");
                    userSeries.StatusChangedDate = DateTime.Now;
                }
                if(userSeries != null && userSeries.WatchStatusId != status.Id)
                {
                    userSeries.WatchStatus = status;
                    userSeries.StatusChangedDate = DateTime.Now;
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
                .Include(ue => ue.UserEpisodes).ThenInclude(e => e.Episode).FirstOrDefaultAsync(x => x.UserId == UserSub);
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
                .Include(ue => ue.UserEpisodes).ThenInclude(e => e.Episode).FirstOrDefaultAsync(x => x.UserId == UserSub);
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
                .FirstOrDefaultAsync(x => x.UserId == UserSub);
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
                UserProfile profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == UserSub);
                profile.ImageSrc = path;
                db.Update(profile);
                await db.SaveChangesAsync();
            }
            return Json("Success");
        }
    }
}
