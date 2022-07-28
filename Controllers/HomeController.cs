using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.Services;
using NotMyShows.ViewModel;
using RestSharp;

namespace NeMyshows.Controllers
{
    public class HomeController : Controller
    {
        readonly SeriesContext db;
        readonly private IWebHostEnvironment _env;
        private readonly ISeriesService _seriesService;
        private readonly IUserService _userService;
        private readonly IUserProfileService _userProfileService;
        public HomeController(SeriesContext context, IWebHostEnvironment env, ISeriesService seriesService, IUserService userService, IUserProfileService userProfileService)
        {
            db = context;
            _env = env;
            _seriesService = seriesService;
            _userService = userService;
            _userProfileService = userProfileService;
        }
        public async Task <IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string UserSub = User.GetSub();
                UserProfile profile = await db.UserProfiles.FirstOrDefaultAsync(x => x.UserId == UserSub);
                return RedirectToAction("Profile", "Profiles", new { id = profile.Id });
            }
            return View();
        }
        public async Task<SeriesCatalogViewModel> GetRecommendations()
        {
            int profileId = await _userService.GetUserProfileIdAsync(_userService.GetUserId());
            await _userProfileService.CreateRecommendations();
            return await _userProfileService.GetRecommendations(profileId);
        }
        [HttpGet]
        public IActionResult GetUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            return Json(User.GetName());
        }
        [HttpGet]
        public async Task<IActionResult> SeriesCatalogAsync()
        {
            SeriesCatalogViewModel model = await _seriesService.GetSeriesListAsync(1, 100);
            return View(model);
        }
        public string GetSafeFilename(string filename)
        {

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }
        public string DownloadImage(Series series, bool isSmall)
        {
            string size = "small_";
            if (!isSmall)
            {
                series.PicturePath = series.PicturePath.Replace("small", "1920");
                size = "normal_";
            }
            string webRoot = Path.Combine(_env.WebRootPath, "images");
            string SafeTitle = GetSafeFilename(series.OriginalTitle).Replace(".", string.Empty).Trim();
            string CoverPath = Path.Combine(webRoot, SafeTitle);
            if (!System.IO.File.Exists(CoverPath))
            {
                Directory.CreateDirectory(CoverPath);
            }
            string PictureName = size + Path.GetFileName(series.PicturePath);
            CoverPath = Path.Combine(CoverPath, PictureName);
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile(series.PicturePath, CoverPath);
            }
            CoverPath = Path.Combine("images", SafeTitle, PictureName);
            return CoverPath;
        }
        [HttpPost]
        [Produces("application/json")]
        public async Task<List<Series>> GetSeries ([FromBody] List<Series> data)
        {
            List<Series> newSeriesList = new List<Series>();
            foreach (var item in data)
            {
                Series series = await db.Series.FirstOrDefaultAsync(x => x.MyShowsId == item.MyShowsId);
                if(series == null)
                {
                    if (item.PicturePath != "")
                    {
                        DownloadImage(item, true);
                        item.PicturePath = DownloadImage(item, false);
                    }
                    var Status = await db.Status.FirstOrDefaultAsync(x => x.Name == item.Status.Name);
                    if (Status == null)
                    {
                        Status = new Status
                        {
                            Name = item.Status.Name
                        };
                        await db.AddAsync(Status);
                        await db.SaveChangesAsync();
                    }
                    item.Status = Status;
                    newSeriesList.Add(item);
                    await db.Series.AddAsync(item);
                }
            }
            await db.SaveChangesAsync();
            return newSeriesList;
        }
        [HttpGet]
        public async Task<string> ParseUsers()
        {
            string url = "https://myshows.me/view/episode/15652293/";
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var doc = await context.OpenAsync(url);
            var users = doc.QuerySelectorAll(".comment-header__author");
            string RequestURL = "https://api.myshows.me/v2/rpc/";
            RestClient client = new RestClient(RequestURL);
            string userLogin = "";
            foreach (var user in users.Take(50))
            {
                List<Series> seriesList = new List<Series>();
                userLogin = user.GetAttribute("href").TrimStart('/');
                var login = new Dictionary<string, string> { { "login", userLogin } };
                var values = new Dictionary<string, object>
                {
                    { "jsonrpc", "2.0" },
                    { "method", "profile.Shows" },
                    {"params" , login },
                    {"id",  1 }
                };
                RestRequest request = new RestRequest();
                request.AddJsonBody(values);
                RestResponse response = await client.ExecutePostAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    dynamic model = JsonConvert.DeserializeObject<dynamic>(response.Content).result;
                    foreach (var item in model)
                    {
                        if (item.rating > 0)
                        {
                            Series series = new Series
                            {
                                MyShowsId = item.show.id,
                                PicturePath = item.show.image,
                                Status = new Status
                                {
                                    Name = item.show.status
                                },
                                Title = item.show.title,
                                OriginalTitle = item.show.titleOriginal,
                                Raiting = new Raitings
                                {
                                    Raiting = item.rating * 2
                                }
                            };
                            seriesList.Add(series);
                        }
                    }
                }
                seriesList = await GetSeries(seriesList.Take(100).ToList());
                Random rand = new Random();
                List<OtherInfoJson> otherInfoList = new List<OtherInfoJson>();
                foreach (var series in seriesList)
                {
                    await Task.Delay(rand.Next(50, 2000));
                    var parameters = new Dictionary<string, object>
                    {
                        { "showId", series.MyShowsId },
                        {"withEpisodes", true }
                    };
                    values = new Dictionary<string, object>
                    {
                        { "jsonrpc", "2.0" },
                        { "method", "shows.GetById" },
                        {"params" , parameters },
                        {"id",  1 }
                    };
                    request = new RestRequest();
                    request.AddJsonBody(values);
                    response = await client.ExecutePostAsync(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        dynamic otherInfo = JsonConvert.DeserializeObject<dynamic>(response.Content).result;
                        List<Episode> episodes = new List<Episode>();
                        foreach (var item in otherInfo.episodes)
                        {
                            try
                            {
                                Episode episode = new Episode
                                {
                                    Title = item.title.ToString(),
                                    ShortName = item.shortName.ToString(),
                                    EpisodeNumber = item.episodeNumber == null ? default(int) : item.episodeNumber,
                                    SeasonNumber = item.seasonNumber == null ? default(int) : item.seasonNumber,
                                    PicturePath = item.image.ToString(),
                                    Date = item.airDate == null ? default(DateTime?) : item.airDate.ToObject<DateTime?>()
                                };
                                episodes.Add(episode);
                            }
                            catch
                            {
                                Debug.Write("Ошибка!!!");
                            }
                        }
                        if (otherInfo.ended == "---" || otherInfo.ended == "")
                        {
                            otherInfo.ended = null;
                        }
                        try
                        {
                            OtherInfoJson otherInfoJson = new OtherInfoJson
                            {
                                MyShowsId = series.MyShowsId,
                                EpisodeTime = otherInfo.runtime == null ? 0 : otherInfo.runtime,
                                TotalTime = otherInfo.runtimeTotal.ToString(),
                                Description = otherInfo.description.ToString(),
                                Channel = new Channel
                                {
                                    Name = otherInfo.network == null ? "" : otherInfo.network.title.ToString()
                                },
                                StartDate = otherInfo.started == null ? default(DateTime) : otherInfo.started,
                                EndDate = otherInfo.ended == null ? default(DateTime?) : otherInfo.ended,
                                GenreIds = otherInfo.genreIds == null ? default(int[]) : otherInfo.genreIds.ToObject<int[]>(),
                                Country = new Country
                                {
                                    Name = otherInfo.country.ToString(),
                                    RussianName = otherInfo.countryTitle.ToString()
                                },
                                Raiting = new Raitings
                                {
                                    KinopoiskId = otherInfo.kinopoiskId == null ? default(int?) : otherInfo.kinopoiskId,
                                    Kinopoisk = otherInfo.kinopoiskRating == null ? default(float) : otherInfo.kinopoiskRating,
                                    ImdbId = otherInfo.imdbId == null ? default(int?) : otherInfo.imdbId,
                                    IMDB = otherInfo.imdbRating == null ? default(float) : otherInfo.imdbRating,
                                    Raiting = series.Raiting.Raiting
                                },
                                Episodes = episodes
                            };
                            otherInfoList.Add(otherInfoJson);
                        }
                        catch (Exception ex)
                        {
                            Debug.Write("Ошибка!!!" + ex);
                        }
                    }
                }
                seriesList = await AddOtherInfo(otherInfoList);
                RegisterModel userRegisterModel = new RegisterModel
                {
                    Email = userLogin + "@gmail.com",
                    Password = "a13814069B_",
                    ConfirmPassword = "a13814069B_"
                };
                UserManagerResponse registerResponse = await _userService.RegisterUserAsync(userRegisterModel, true);
                if (registerResponse.IsSuccess)
                {
                    List<UserSeries> userSeriesList = new List<UserSeries>();
                    foreach (Series series in seriesList)
                    {
                        UserSeries userSeries = new UserSeries
                        {
                            UserProfileId = registerResponse.UserProfileId,
                            SeriesId = series.Id,
                            UserRaiting = (int)series.Raiting.Raiting,
                            WatchStatusId = 1,
                            StatusChangedDate = DateTime.Now,
                            RaitingDate = DateTime.Now
                        };
                        userSeriesList.Add(userSeries);
                    }
                    await db.AddRangeAsync(userSeriesList);
                    await db.SaveChangesAsync();
                }

            }
            await db.SaveChangesAsync();
            return "Success";
        }
        [HttpPost]
        [Produces("application/json")]
        public async Task<string> GetGenres([FromBody] List<Genre> data)
        {
            foreach (var genre in data)
            {
                await db.Genre.AddAsync(genre);
                await db.SaveChangesAsync();
            }
            return "Список жанров получен";
        }
        [HttpPost]
        public async Task<int []> GetSeriesIds(int startId, int EndId)
        {
            int [] MyShowsIds =await db.Series.Where(x => x.Id >= startId && x.Id <= EndId).Select(x=>x.MyShowsId).ToArrayAsync();
            return MyShowsIds;
        }
        [HttpPost]
        [Produces("application/json")]
        public async Task<List<Series>> AddOtherInfo([FromBody] List<OtherInfoJson> data)
        {
            List<Series> Series = new List<Series>();
            foreach (var item in data)
            {
                try
                {
                    Series series = await db.Series.FirstOrDefaultAsync(x => x.MyShowsId == item.MyShowsId);
                    series.Description = item.Description.Substring(0, item.Description.IndexOf("</p>") + 4);
                    series.EpisodeTime = item.EpisodeTime;
                    series.TotalTime = item.TotalTime;
                    Channel channel = await db.Channel.FirstOrDefaultAsync(x => x.Name == item.Channel.Name);
                    if (channel == null)
                    {
                        channel = new Channel
                        {
                            Name = item.Channel.Name
                        };
                        await db.AddAsync(channel);
                        await db.SaveChangesAsync();
                    }
                    series.Channel = channel;
                    series.StartDate = item.StartDate;
                    series.EndDate = item.EndDate;
                    for (int i = 0; i < item.GenreIds.Count(); i++)
                    {
                        SeriesGenres genre = new SeriesGenres
                        {
                            GenreId = item.GenreIds[i]
                        };
                        series.SeriesGenres.Add(genre);
                    }
                    Country country = await db.Country.FirstOrDefaultAsync(x => x.Name == item.Country.Name);
                    if (country == null)
                    {
                        country = new Country
                        {
                            Name = item.Country.Name,
                            RussianName = item.Country.RussianName
                        };
                        await db.AddAsync(country);
                        await db.SaveChangesAsync();
                    }
                    series.Country = country;
                    series.Raiting = item.Raiting;
                    if(series.Raiting.Raiting == 0)
                    {
                        series.Raiting.Raiting = item.Raiting.IMDB;
                    }
                    series.Raiting.Votes = 1;
                    series.Episodes = item.Episodes;
                    Series.Add(series);
                }

                catch(Exception ex)
                {
                    Debug.Write("Ошибка!!!" + ex);
                }
            }
            db.UpdateRange(Series);
            await db.SaveChangesAsync();
            return Series;
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
public static class ClaimsPrincipalExtensions
{
    public static string GetSub(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(x => x.Type.Equals(ClaimTypes.NameIdentifier))?.Value;
    }
    public static string GetEmail(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(x => x.Type.Equals(ClaimTypes.Email))?.Value;
    }
    public static string GetName(this ClaimsPrincipal principal)
    {
        return principal?.FindFirst(x => x.Type.Equals("name"))?.Value;
    }
}