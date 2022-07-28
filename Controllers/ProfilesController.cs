using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.Services;
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
        private readonly IUserProfileService _userProfileService;
        private readonly ISeriesService _seriesService;
        public ProfilesController(SeriesContext context, IWebHostEnvironment env, IUserProfileService userProfileService, ISeriesService seriesService)
        {
            db = context;
            _env = env;
            _userProfileService = userProfileService;
            _seriesService = seriesService;
        }
        public async Task<IActionResult> Profile(int Id)
        {
            UserProfileViewModel model = await _userProfileService.GetUserProfileAsync(Id);
            if(model == null)
            {
                return BadRequest();
            }
            return View("UserProfile", model);
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
        public async Task<IActionResult> Episode(int EpisodeId)
        {
            EpisodeViewModel model = await _seriesService.GetEpisodeAsync(EpisodeId);
            return View(model);
        }
        public async Task<IActionResult> Friends()
        {
            FriendsViewModel model = await _userProfileService.GetFriends();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(string CommentText, int EpisodeId)
        {
            await _userProfileService.AddComment(CommentText, EpisodeId);
            return Json("Success");
        }
        [HttpPost]
        public async Task AddFriend(int FriendId)
        {
            await _userProfileService.AddFriend(FriendId);
        }
        [HttpPost]
        public async Task RemoveFriend(int FriendId)
        {
            await _userProfileService.RemoveFriend(FriendId);
        }
        public async Task<IActionResult> Series(int SeriesId)
        {

            SeriesViewModel model = await _seriesService.GetSeriesAsync(SeriesId);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SelectWatchStatus(int SeriesId, string StatusName)
        {
            await _userProfileService.SelectWatchStatus(SeriesId, StatusName);
            return Json("Success");
        }
        [HttpPost]
        public async Task<IActionResult> CheckEpisodes(int[] CheckedIds, int SeriesId)
        {
            await _userProfileService.CheckEpisodes(CheckedIds, SeriesId);
            return Json("Success");
        }
        [HttpPost]
        public async Task<float> SelectSeriesRaiting (int UserRaiting, int SeriesId)
        {
           return await _userProfileService.SelectSeriesRaiting(UserRaiting, SeriesId);
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
                UserProfile profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == UserSub);
                profile.ImageSrc = path;
                db.Update(profile);
                await db.SaveChangesAsync();
            }
            return Json("Success");
        }
    }
}
