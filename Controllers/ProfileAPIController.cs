using Microsoft.AspNetCore.Mvc;
using NotMyShows.Services;
using NotMyShows.ViewModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NotMyShows.Models;
using System.Collections.Generic;

namespace NotMyShows.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProfileAPIController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        public ProfileAPIController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }
        [HttpGet]
        public async Task<UserProfileViewModel> GetUserProfile(int id)
        {
            UserProfileViewModel userProfile = await _userProfileService.GetUserProfileAsync(id);
            return userProfile;
        }
        [HttpPost]
        public async Task<float> SelectSeriesRaiting(RaitingRequestParams parameters)
        {
            return await _userProfileService.SelectSeriesRaiting(parameters.UserRaiting, parameters.SeriesId);
        }
        [HttpPost]
        public async Task<string> SelectWatchStatus(WatchStatusRequestParams parameters)
        {
            await _userProfileService.SelectWatchStatus(parameters.SeriesId, parameters.StatusName);
            return "Success";
        }
        [HttpPost]
        public async Task<string> CheckEpisodes(CheckEpisodesRequestParams parameters)
        {
            await _userProfileService.CheckEpisodes(parameters.CheckedIds, parameters.SeriesId);
            return "Success";
        }
        [HttpGet]
        public async Task<SeriesCatalogViewModel> GetRecommendations(int ProfileId)
        {
            return await _userProfileService.GetRecommendations(ProfileId);
        }
        [HttpGet]
        public async Task AddFriend(int FriendId)
        {
            await _userProfileService.AddFriend(FriendId);
        }
        [HttpGet]
        public async Task<FriendsViewModel> GetFriends()
        {
            return await _userProfileService.GetFriends();
        }
        [HttpGet]
        public async Task RemoveFriend(int FriendId)
        {
            await _userProfileService.RemoveFriend(FriendId);
        }
        [HttpPost]
        public async Task<Comment> AddComment(CommentRequestParams parameters)
        {
            return await _userProfileService.AddComment(parameters.CommentText, parameters.EpisodeId);
        }
    }
    public class CommentRequestParams
    {
        public int EpisodeId { get; set; }
        public string CommentText { get; set; }
    }
    public class CheckEpisodesRequestParams
    {
        public int SeriesId { get; set; }
        public int[] CheckedIds { get; set; }
    }
    public class WatchStatusRequestParams
    {
        public int SeriesId { get; set; }
        public string StatusName { get; set; }
    }
    public class RaitingRequestParams
    {
        public int SeriesId { get; set; }
        public int UserRaiting { get; set; }
    }
}
