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
    public interface IUserProfileService
    {
        Task<UserProfileViewModel> GetUserProfileAsync(int id);
        Task<float> SelectSeriesRaiting(int UserRaiting, int SeriesId);
        Task SelectWatchStatus(int SeriesId, string StatusName);
        Task CheckEpisodes(int[] CheckedIds, int SeriesId);
        Task<Comment> AddComment(string CommentText, int EpisodeId);
        Task CreateRecommendations();
        Task AddFriend(int FriendId);
        Task RemoveFriend(int FriendId);
        Task<FriendsViewModel> GetFriends();
        Task<SeriesCatalogViewModel> GetRecommendations(int UserProfileId);
    }
    public class UserProfileService : IUserProfileService
    {
        readonly SeriesContext _context;
        private readonly IUserService _userService;
        public UserProfileService(SeriesContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }
        public async Task<ProfileData> GetProfileData(int id)
        {
            ProfileData profile = await _context.UserProfiles.AsNoTracking().Select(p => new ProfileData
            {
                Id = p.Id,
                UserId = p.UserId,
                Name = p.User.UserName,
                ImageSrc = p.ImageSrc,
                UserSeries = p.UserSeries.Select(us => new ProfileSeriesItem
                {
                    Id = us.SeriesId,
                    Title = us.Series.Title,
                    OriginalTitle = us.Series.OriginalTitle,
                    EpisodesCount = us.Series.Episodes.Count(x => x.EpisodeNumber != 0),
                    EpisodeTime = us.Series.EpisodeTime,
                    WatchStatusId = us.WatchStatusId,
                    Status = new SeriesStatus
                    {
                        Name = StatusColor.GetNewStatusName(us.Series.Status.Name),
                        StatusColorName = StatusColor.GetColor(us.Series.Status.Name)
                    },
                    PicturePath = us.Series.PicturePath,
                    UserRaiting = us.UserRaiting,
                    StatusChangedDate = us.StatusChangedDate,
                    RaitingDate = us.RaitingDate,
                    UserEpisodes = us.UserProfile.UserEpisodes
                    .Where(x => x.Episode.SeriesId == us.Series.Id && x.Episode.EpisodeNumber != 0)
                    .Select(x => new UserEpisodeData
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
            }).FirstOrDefaultAsync(x => x.Id == id);
            return profile;
        } 
        public async Task<IEnumerable<WatchStatus>> GetWatchStatusesAsync()
        {
            IEnumerable<WatchStatus> watchStatuses = await _context.WatchStatuses.AsNoTracking().ToListAsync();
            return watchStatuses;
        }
        public List<WatchStatusTab> CreateStatusTabs(ProfileData profile, IEnumerable<WatchStatus> watchStatuses)
        {
            // Группмпуем сериалы пользователя по статусам
            var seriesGroups = profile.UserSeries.GroupBy(s => s.WatchStatusId);
            List<WatchStatusTab> statusTabs = new List<WatchStatusTab>();
            foreach (var status in watchStatuses)
            {
                // Список сериалов для текущего статуса (таба)
                List<ProfileSeriesItem> TabSeriesList = new List<ProfileSeriesItem>();
                if (seriesGroups.Any())
                {
                    var tempSeriesList = seriesGroups.FirstOrDefault(x => x.Key == status.Id);
                    if (tempSeriesList != null)
                    {
                        TabSeriesList = tempSeriesList.ToList();
                    }
                }
                // Создаем таб
                WatchStatusTab tab = new WatchStatusTab
                {
                    WatchStatus = status,
                    SeriesList = TabSeriesList
                };
                statusTabs.Add(tab);
            }
            return statusTabs;
        }
        public ProfileStats GetProfileStats(ProfileData profile, List<WatchStatusTab> statusTabs)
        {
            int hoursSpent = profile.UserSeries.Sum(x => x.UserEpisodes.Count() * x.EpisodeTime) / 60;
            ProfileStats profileStats = new ProfileStats
            {
                EpisodesCount = profile.UserSeries.Sum(x => x.UserEpisodes.Count()),
                SeriesCount = statusTabs.FirstOrDefault(x => x.WatchStatus.StatusName == "Просмотрено").SeriesList.Count(),
                HoursSpent = hoursSpent,
                DaysSpent = hoursSpent/24,
                AchievementsCount = 0
            };
            return profileStats;
        }
        public bool IsProfileOwner(string userId)
        {
            if (_userService.IsAuthenticated())
            {
                string currentUserId = _userService.GetUserId();
                return currentUserId == userId;
            }
            return false;
        }
        public async Task<bool> IsFriendAsync(int profileId, bool isProfileOwner)
        {
            if (!isProfileOwner)
            {
                if (_userService.IsAuthenticated())
                {
                    string userId = _userService.GetUserId();
                    UserProfile userProfile = await _context.UserProfiles.Include(f => f.Friends).FirstOrDefaultAsync(x => x.UserId == userId);
                    return userProfile.Friends.FirstOrDefault(x => x.FriendProfileId == profileId) == null ? false : true;
                }
            }
            return false;
        }
        // Получаем события пользователя
        public async Task<IEnumerable<EventGroup>> CreateUserEventsAsync(IEnumerable<ProfileSeriesItem> SeriesItems, IEnumerable<Friend> Friends, IEnumerable<WatchStatus> watchStatuses)
        {
            List<UserEvent> userEvents = new List<UserEvent>();
            // События, связанные с сериалами
            foreach (var series in SeriesItems)
            {
                // Изменение статуса просмотра сериала
                UserEvent seriesEvent = new SeriesEvent
                {
                    SeriesId = series.Id,
                    SeriesTitle = series.Title,
                    Date = series.StatusChangedDate,
                    WatchStatus = watchStatuses.FirstOrDefault(x => x.Id == series.WatchStatusId).StatusName
                };
                userEvents.Add(seriesEvent);
                // Оценка сериала 
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
                // Группировка событий по просмотренным эпизодам
                // Если в течение 5 минут было отмечено несколько эпизодов, то эти события группируются
                var groupedEpisodes = series.UserEpisodes
                    .GroupBy(i => i.WatchDate.Year, (k, g) => g
                    .GroupBy(i => (long)(i.WatchDate - g.Min(e => e.WatchDate)).TotalMinutes / 5))
                    .SelectMany(g => g);
                foreach (var group in groupedEpisodes)
                {
                    List<UserEpisodeData> episodesList = group.ToList();
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
            // Взаимодействие с другими пользователями
            foreach (var friend in Friends)
            {
                UserEvent friendEvent = new FriendEvent
                {
                    UserName = await _userService.GetUserNameByIdAsync(friend.FriendProfileId),
                    UserProfileId = friend.FriendProfileId,
                    Date = friend.Date
                };
                userEvents.Add(friendEvent);
            }
            // Сортируем события по дате в обратном порядке
            userEvents.Sort((x, y) => x.Date.CompareTo(y.Date));
            userEvents.Reverse();
            // группируем события по дням, учитывая их год и месяц
            var groupedEvents = userEvents.GroupBy(x => new
            {
                x.Date.Year,
                x.Date.DayOfYear,
            });
            List<EventGroup> groupedUserEvents = new List<EventGroup>();
            foreach (var group in groupedEvents)
            {
                EventGroup eventGroup = new EventGroup
                {
                    Date = group.First().Date,
                    UserEvents = group.ToList()
                };
                groupedUserEvents.Add(eventGroup);
            }
            return groupedUserEvents;
        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(int id)
        {
            // Получаем статусы просмотра (смотрю, буду смотреть и т.д.)
            IEnumerable<WatchStatus> watchStatuses = await GetWatchStatusesAsync();
            // Получаем данные о профиле
            ProfileData profile = await GetProfileData(id);
            if(profile != null && watchStatuses != null)
            {
                // Сортируем сериалы по статусам и создаем табы
                List<WatchStatusTab> statusTabs = CreateStatusTabs(profile, watchStatuses);
                // Получаем статистику пользователя
                ProfileStats profileStats = GetProfileStats(profile, statusTabs);
                // Является ли текущий пользователь владельцем запрашиваемого профиля
                bool isProfileOwner = IsProfileOwner(profile.UserId);
                UserProfileViewModel model = new UserProfileViewModel
                {
                    Id = profile.Id,
                    UserName = profile.Name,
                    ImageSrc = profile.ImageSrc,
                    IsFriend = await IsFriendAsync(profile.Id, isProfileOwner),
                    StatusTabs = statusTabs,
                    ProfileStats = profileStats,
                    IsProfileOwner = isProfileOwner,
                    GroupedUserEvents = await CreateUserEventsAsync(profile.UserSeries, profile.Friends, watchStatuses)
                };
                return model;
            }
            return default;
        }
        public async Task<float> SelectSeriesRaiting(int UserRaiting, int SeriesId)
        {
            string UserSub = _userService.GetUserId();
            UserProfile userProfile = await _context.UserProfiles.Include(us => us.UserSeries)
                .ThenInclude(s => s.Series).ThenInclude(r => r.Raiting)
                .FirstOrDefaultAsync(x => x.UserId == UserSub);
            UserSeries userSeries = userProfile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
            if (userSeries != null)
            {
                int PrevUserRaiting = userSeries.UserRaiting;
                float PrevSeriesRaiting = userSeries.Series.Raiting.Raiting;
                int Votes = userSeries.Series.Raiting.Votes;
                if (PrevUserRaiting == 0 && UserRaiting != 0)
                {
                    userSeries.Series.Raiting.Raiting = (float)(PrevSeriesRaiting * Votes + UserRaiting) / (Votes + 1);
                    userSeries.Series.Raiting.Votes++;
                }
                else if(PrevUserRaiting > 0 && UserRaiting == 0)
                {
                    userSeries.Series.Raiting.Raiting = (float)(PrevSeriesRaiting * Votes - PrevUserRaiting) / (Votes - 1);
                    userSeries.Series.Raiting.Votes--;
                }
                else if(PrevUserRaiting > 0 && UserRaiting != 0)
                {
                    userSeries.Series.Raiting.Raiting = (float)(PrevSeriesRaiting * Votes - PrevUserRaiting + UserRaiting) / Votes;
                }
                userSeries.UserRaiting = UserRaiting;
                userSeries.RaitingDate = DateTime.Now;
                _context.Update(userSeries);
                await _context.SaveChangesAsync();
                return userSeries.Series.Raiting.Raiting;
            }
            Series series = await _context.Series.Include(r => r.Raiting).FirstOrDefaultAsync(x => x.Id == SeriesId);
            return series.Raiting.Raiting;
        }
        public async Task SelectWatchStatus(int SeriesId, string StatusName)
        {
            string UserSub = _userService.GetUserId();
            UserProfile userProfile = await _context.UserProfiles.Include(us => us.UserSeries).FirstOrDefaultAsync(x => x.UserId == UserSub);
            if (userProfile != null)
            {
                WatchStatus status = await _context.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == StatusName);
                if(status != null)
                {
                    UserSeries userSeries = userProfile.UserSeries.FirstOrDefault(x => x.SeriesId == SeriesId);
                    if (userSeries != null)
                    {
                        //if (userSeries.WatchStatusId == status.Id)
                        //{
                        //    userSeries.WatchStatus = await _context.WatchStatuses.FirstOrDefaultAsync(x => x.StatusName == "Брошено");
                        //    userSeries.StatusChangedDate = DateTime.Now;
                        //}
                        if (userSeries.WatchStatusId != status.Id)
                        {
                            if(status.StatusName == "Не смотрю")
                            {
                                _context.Remove(userSeries);
                            }
                            else
                            {
                                userSeries.WatchStatus = status;
                                userSeries.StatusChangedDate = DateTime.Now;
                                _context.Update(userSeries);
                            }
                        }
                    }
                    if (userSeries == null && status.StatusName != "Не смотрю")
                    {
                        userSeries = new UserSeries
                        {
                            SeriesId = SeriesId,
                            WatchStatus = status,
                            StatusChangedDate = DateTime.Now
                        };
                        userProfile.UserSeries.Add(userSeries);
                        _context.Update(userProfile);
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }
        public async Task CheckEpisodes(int[] CheckedIds, int SeriesId)
        {
            string UserSub = _userService.GetUserId();
            var profile = await _context.UserProfiles.Select(p => new
            {
                Id = p.Id,
                UserId = p.UserId,
                UserEpisodes = p.UserEpisodes.Where(x => x.Episode.SeriesId == SeriesId)
                .Select(ue => new UserEpisodes
                {
                    EpisodeId = ue.EpisodeId,
                    Episode = ue.Episode
                }).ToList()
            }).FirstOrDefaultAsync(x => x.UserId == UserSub);
            var AlreadyChecked = profile.UserEpisodes.Where(x => CheckedIds.Contains(x.EpisodeId));
            List<UserEpisodes> episodes = new List<UserEpisodes>();
            // Добавляем/удаляем эпизоды
            if (AlreadyChecked.Any())
            {
                if (AlreadyChecked.Count() == CheckedIds.Count())
                {
                    foreach (int id in CheckedIds)
                    {
                        UserEpisodes episode = new UserEpisodes
                        {
                            UserProfileId = profile.Id,
                            EpisodeId = id
                        };
                        episodes.Add(episode);
                    }
                    _context.RemoveRange(episodes);
                }
                else
                {
                    foreach (int id in CheckedIds)
                    {
                        UserEpisodes episode = AlreadyChecked.FirstOrDefault(x => x.EpisodeId == id);
                        if (episode == null)
                        {
                            episode = new UserEpisodes
                            {
                                UserProfileId = profile.Id,
                                EpisodeId = id,
                                WatchDate = DateTime.Now
                            };
                            episodes.Add(episode);
                        }
                    }
                    await _context.AddRangeAsync(episodes);
                }
            }
            else
            {
                foreach (int id in CheckedIds)
                {
                    UserEpisodes episode = new UserEpisodes
                    {
                        UserProfileId = profile.Id,
                        EpisodeId = id,
                        WatchDate = DateTime.Now
                    };
                    episodes.Add(episode);
                }
                await _context.AddRangeAsync(episodes);
            }
            int TotalCount = _context.Episodes.Count(x => x.SeriesId == SeriesId && x.EpisodeNumber != 0);
            await _context.SaveChangesAsync();
            var WatchedCount = await _context.UserProfiles.AsNoTracking().Select(p => new
            {
                UserId = p.UserId,
                Count = p.UserEpisodes.Count(x => x.Episode.SeriesId == SeriesId && x.Episode.EpisodeNumber != 0)
            }).FirstOrDefaultAsync(x => x.UserId == UserSub);
            if (TotalCount == WatchedCount.Count)
            {
                await SelectWatchStatus(SeriesId, "Просмотрено");
            }
            else
            {
                await SelectWatchStatus(SeriesId, "Смотрю");
            }
        }
        public async Task<Comment> AddComment(string CommentText, int EpisodeId)
        {
            string UserSub = _userService.GetUserId();
            UserProfile profile = await _context.UserProfiles.Include(c => c.Comments).FirstOrDefaultAsync(x => x.UserId == UserSub);
            Comment comment = new Comment
            {
                EpisodeId = EpisodeId,
                Text = CommentText,
                Likes = 0,
                Dislikes = 0,
                Date = DateTime.Now
            };
            profile.Comments.Add(comment);
            _context.Update(profile);
            await _context.SaveChangesAsync();
            return comment;
        }
        public async Task AddFriend(int FriendId)
        {
            string UserSub = _userService.GetUserId();
            UserProfile profile = await _context.UserProfiles.Include(f => f.Friends).FirstOrDefaultAsync(x => x.UserId == UserSub);
            if (profile.Friends.FirstOrDefault(x => x.FriendProfileId == FriendId) == null)
            {
                Friend friend = new Friend
                {
                    FriendProfileId = FriendId,
                    Date = DateTime.Now
                };
                profile.Friends.Add(friend);
                _context.Update(profile);
                await _context.SaveChangesAsync();
            }
        }
        public async Task RemoveFriend(int FriendId)
        {
            string UserSub = _userService.GetUserId();
            UserProfile profile = await _context.UserProfiles.Include(f => f.Friends).FirstOrDefaultAsync(x => x.UserId == UserSub);
            Friend friend = profile.Friends.FirstOrDefault(x => x.FriendProfileId == FriendId);
            if (friend != null)
            {
                profile.Friends.Remove(friend);
                _context.Update(profile);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<FriendsViewModel> GetFriends()
        {
            string UserSub = _userService.GetUserId();
            var profile = await _context.UserProfiles.Include(f => f.Friends)
                .AsNoTracking().FirstOrDefaultAsync(x => x.UserId == UserSub);
            List<int> profileIds = new List<int>();
            foreach (var item in profile.Friends)
            {
                profileIds.Add(item.FriendProfileId);
            }
            List<UserProfile> friendProfiles = await _context.UserProfiles.Include(u => u.User).Where(x => profileIds.Contains(x.Id)).AsNoTracking().ToListAsync();
            FriendsViewModel model = new FriendsViewModel
            {
                Friends = friendProfiles
            };
            return model;
        }
        public async Task CreateRecommendations()
        {
            List<Series> seriesList = await _context.Series.Include(sg => sg.SeriesGenres).ThenInclude(g => g.Genre).ToListAsync();
            List<UserProfile> profiles = await _context.UserProfiles.Include(x => x.UserSeries).ToListAsync();
            List<UserRaitings> RaitingsMatrix = new List<UserRaitings>();
            foreach (UserProfile profile in profiles)
            {
                UserRaitings user = new UserRaitings()
                {
                    UserProfileId = profile.Id,
                    Raitings = new List<SeriesRaiting>()
                };
                foreach (Series series in seriesList)
                {
                    UserSeries currentSeries = profile.UserSeries.FirstOrDefault(x => x.SeriesId == series.Id);
                    if (currentSeries != null)
                    {
                        user.Raitings.Add(
                            new SeriesRaiting()
                            {
                                SeriesId = currentSeries.SeriesId,
                                Raiting = currentSeries.UserRaiting
                            });
                    }
                    else
                    {
                        user.Raitings.Add(
                            new SeriesRaiting()
                            {
                                SeriesId = series.Id,
                                Raiting = 0
                            });
                    }
                }
                RaitingsMatrix.Add(user);
            }
            SVDPP svdpp = new SVDPP();
            RaitingsMatrix = svdpp.Start(RaitingsMatrix);
            List<UserRecommendation> recommendations = new List<UserRecommendation>();
            foreach (UserRaitings user in RaitingsMatrix)
            {
                UserProfile currentProfile = profiles.FirstOrDefault(x => x.Id == user.UserProfileId);
                List<string> userGenres = new List<string>();
                foreach(var us in currentProfile.UserSeries)
                {
                    List<string> genre = seriesList.FirstOrDefault(x => x.Id == us.SeriesId).SeriesGenres.Select(x => x.Genre.Name).ToList();
                    if(genre != null)
                    {
                        foreach (string name in genre)
                        {
                            if (!userGenres.Contains(name))
                            {
                                userGenres.Add(name);
                            }
                        }
                    }
                }
                foreach (SeriesRaiting series in user.Raitings.Where(x => x.Raiting >= 7))
                {
                    if(currentProfile.UserSeries.FirstOrDefault(x => x.SeriesId == series.SeriesId) == null)
                    {
                        List<string> currentGenres = seriesList.FirstOrDefault(x => x.Id == series.SeriesId).SeriesGenres.Select(x => x.Genre.Name).ToList();
                        if (currentGenres != null)
                        {
                            if (userGenres.Count(currentGenres.Contains) >= currentGenres.Count)
                            {
                                if (series.Raiting > 10)
                                {
                                    series.Raiting = 10;
                                }
                                UserRecommendation recommendation = new UserRecommendation()
                                {
                                    SeriesId = series.SeriesId,
                                    UserProfileId = user.UserProfileId,
                                    PotentialRating = (float)series.Raiting,
                                    Date = DateTime.Now
                                };
                                recommendations.Add(recommendation);
                            }
                        }
                    }
                }
            }
            await _context.AddRangeAsync(recommendations);
            await _context.SaveChangesAsync();
        }

        public async Task<SeriesCatalogViewModel> GetRecommendations(int UserProfileId)
        {
            var profile = await _context.UserProfiles
                .Include(x => x.UserRecommendations).ThenInclude(s => s.Series).ThenInclude(s => s.Raiting)
                .Select(p => new 
                {
                    ProfileId = p.Id,
                    UserRecommendation = p.UserRecommendations.Select(r => new UserRecommendation 
                    {
                        SeriesId = r.SeriesId,
                        Series = r.Series,
                        PotentialRating = r.PotentialRating
                    })
                }).AsNoTracking().FirstOrDefaultAsync(x => x.ProfileId == UserProfileId);
            IEnumerable<SeriesCatalogItem> series = profile.UserRecommendation.OrderByDescending(x => x.PotentialRating).Take(100)
                .Select(s => new SeriesCatalogItem
                {
                    SeriesId = s.SeriesId,
                    Title = s.Series.Title,
                    OriginalTitle = s.Series.OriginalTitle,
                    PicturePath = s.Series.PicturePath,
                    Raiting = s.Series.Raiting.Raiting,
                    PotentialRating = (int)(s.PotentialRating*10)
                });

            SeriesCatalogViewModel model = new SeriesCatalogViewModel
            {
                SeriesListView = series
            };
            return model;
        }
    }
    public class UserRaitings
    {
        public int UserProfileId { get; set; }
        public List<SeriesRaiting> Raitings { get; set; }
    }
    public class SeriesRaiting
    {
        public double Raiting { get; set; }
        public int SeriesId { get; set; }
    }
}
