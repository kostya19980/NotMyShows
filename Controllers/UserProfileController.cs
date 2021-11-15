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
    public class UserProfileController : Controller
    {
        readonly SeriesContext db;
        readonly private IHostingEnvironment _env;
        public UserProfileController(SeriesContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }
        public async Task<IActionResult> UserProfile()
        {
            UserProfile profile = await GetUserProfile(false);
            List<ViewingStatus> viewingStatuses = await db.ViewingStatuses.ToListAsync();
            List<StatusViewModel> statusViewModels = new List<StatusViewModel>();
            foreach(var item in viewingStatuses)
            {
                StatusViewModel statusViewModel = new StatusViewModel
                {
                    ViewingStatus = item,
                    SeriesCount = await GetSeriesCount(item.Id)
                };
                statusViewModels.Add(statusViewModel);
            }
            UserProfileViewModel model = new UserProfileViewModel
            {
                UserProfile = profile,
                ViewingStatuses = statusViewModels
            };
            return View(model);
        }
        public async Task<int> GetSeriesCount(int viewingStatusId)
        {
            UserProfile profile = await GetUserProfile(true);
            int SeriesCount = profile.UserSeries.Where(x => x.ViewingStatusId == viewingStatusId).Count();
            return SeriesCount;
        }
        public async Task<UserProfile> GetUserProfile(bool IncludeSeries)
        {
            string UserSub = User.GetSub();
            UserProfile profile = new UserProfile();
            if (IncludeSeries)
            {
                profile = await db.UserProfiles.Include(s => s.UserSeries).ThenInclude(s => s.Series).ThenInclude(s => s.Status)
                    .FirstOrDefaultAsync(x => x.UserSub == UserSub);
            }
            else
            {
                profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserSub == UserSub);
            }
            return profile;
        }
        public async Task<List<SeriesView>> GetUserSeries(int viewingStatusId)
        {
            UserProfile profile = await GetUserProfile(true);
            var userSeries = profile.UserSeries.Where(x => x.ViewingStatusId == viewingStatusId);
            List<SeriesView> seriesListView = new List<SeriesView>();
            foreach (var item in userSeries)
            {
                SeriesView seriesView = new SeriesView(item.Series);
                seriesListView.Add(seriesView);
            }
            return seriesListView;
        }
        [HttpPost]
        public async Task<IActionResult> _ProfileSeries(int StatusID)
        {
            List<SeriesView> seriesViews = await GetUserSeries(StatusID);
            SeriesViewModel model = new SeriesViewModel
            {
                SeriesListView = seriesViews
            };
            return PartialView(model);
        }
        public async Task<IActionResult> Series(int SeriesId)
        {
            Series series = await db.Series.Include(r => r.Raiting).Include(s => s.Status).Include(ch => ch.Channel)
                .Include(c => c.Country).Include(sg => sg.SeriesGenres).ThenInclude(g=>g.Genre).Include(e => e.Episodes)
                .FirstOrDefaultAsync(x => x.Id == SeriesId);
            series.Episodes.Sort((x, y) => DateTime.Compare(x.Date ?? DateTime.MaxValue, y.Date ?? DateTime.MaxValue));
            if (series == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            SeriesView model = new SeriesView(series);
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddSeriesAsync(int SeriesId, string StatusName)
        {
            //List<ViewingStatus> list = await db.ViewingStatuses.ToListAsync();
            //if (list.Count == 0)
            //{
            //    string[] names = { "Смотрю", "Запланировано", "Отложено", "Брошено", "Просмотрено" };
            //    foreach (var name in names)
            //    {
            //        ViewingStatus status = new ViewingStatus
            //        {
            //            StatusName = name
            //        };
            //        await db.AddAsync(status);
            //        await db.SaveChangesAsync();
            //    }
            //}
            string UserSub = User.GetSub();
            UserProfile userProfile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserSub == UserSub);
            //if (userProfile == null)
            //{
            //    userProfile = new UserProfile
            //    {
            //        UserSub = UserSub
            //    };
            //    await db.AddAsync(userProfile);
            //    await db.SaveChangesAsync();
            //}
            if (userProfile != null)
            {
                ViewingStatus status = await db.ViewingStatuses.FirstOrDefaultAsync(x => x.StatusName == StatusName);
                UserSeries userSeries = new UserSeries
                {
                    SeriesId = SeriesId,
                    ViewingStatus = status
                };
                userProfile.UserSeries.Add(userSeries);
                db.Update(userProfile);
                await db.SaveChangesAsync();
            }
            return Json(userProfile);
        }
    }
}
