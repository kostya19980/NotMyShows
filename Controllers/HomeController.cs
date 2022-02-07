using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotMyShows.Models;
using NotMyShows.ViewModel;

namespace NeMyshows.Controllers
{
    public class HomeController : Controller
    {
        readonly SeriesContext db;
        readonly private IHostingEnvironment _env;
        public HomeController(SeriesContext context, IHostingEnvironment env)
        {
            db = context;
            _env = env;
        }
        public async Task <IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string UserSub = User.GetSub();
                int id = db.UserProfiles.FirstOrDefaultAsync(x => x.UserSub == UserSub).Result.Id;
                return RedirectToAction("Profile", "Profiles", new { id = id });
            }
            return View();
        }
        [Authorize]
        public ActionResult Secret()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetUserAsync()
        {
            var identity = (ClaimsIdentity)User.Identity;
            return Json(User.GetName());
        }
        public IActionResult Login()
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.Headers["Referer"].ToString()
            }, OpenIdConnectDefaults.AuthenticationScheme);
        }
        public IActionResult Register()
        {
            return Redirect("https://localhost:9001/Account/Register");
            //return Challenge(new AuthenticationProperties
            //{
            //    RedirectUri = Request.Headers["Referer"].ToString()
            //}, OpenIdConnectDefaults.AuthenticationScheme);
        }
        public ActionResult Logout()
        {
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme,OpenIdConnectDefaults.AuthenticationScheme);
        }
        [HttpGet]
        public async Task<IActionResult> SeriesSearch()
        {
            //List<Series> series = await db.Series.Include(r=>r.Raiting).Include(s=>s.Status).Include(ch=>ch.Channel).Include(c => c.Country).
            //    Include(sg => sg.SeriesGenres).ThenInclude(g=>g.Genre).
            //    Where(x => x.Id >= 1 && x.Id <= 100).ToListAsync();
            List<Series> series = await db.Series.Include(s => s.Status)
                .Where(x => x.Id >= 1 && x.Id <= 100).ToListAsync();
            List<SeriesView> seriesListView = new List<SeriesView>();
            foreach(var item in series)
            {
                SeriesView seriesView = new SeriesView
                {
                    Series = item,
                    StatusColorName = StatusColor.GetColor(item.Status.Name)
                };
                seriesView.Series.Status.Name = StatusColor.GetNewStatusName(item.Status.Name);
                seriesListView.Add(seriesView);
            }
            SeriesViewModel model = new SeriesViewModel
            {
                SeriesListView = seriesListView
            };
            return View(model);
        }
        public string GetSafeFilename(string filename)
        {

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));

        }
        [HttpPost]
        [Produces("application/json")]
        public async Task<string> GetSeries ([FromBody] List<Series> data)
        {
            string webRoot = Path.Combine(_env.WebRootPath, "images");
            foreach (var item in data){
                using (WebClient webClient = new WebClient())
                {
                    if (item.PicturePath != "")
                    {
                        string SafeTitle = GetSafeFilename(item.OriginalTitle);
                        string CoverPath = Path.Combine(webRoot, SafeTitle);
                        if (!System.IO.File.Exists(CoverPath))
                        {
                            Directory.CreateDirectory(CoverPath);
                        }
                        string PictureName = Path.GetFileName(item.PicturePath);
                        CoverPath = Path.Combine(CoverPath, PictureName);
                        webClient.DownloadFile(item.PicturePath, CoverPath);
                        CoverPath = Path.Combine("images", SafeTitle, PictureName);
                        item.PicturePath = CoverPath;
                    }
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
                await db.Series.AddAsync(item);
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
        public async Task<string> AddOtherInfo([FromBody] List<OtherInfoJson> data)
        {
            //string webRoot = Path.Combine(_env.WebRootPath, "images");
            List<Series> Series = new List<Series>();
            foreach (var item in data)
            {
                Series series = await db.Series.FirstOrDefaultAsync(x => x.MyShowsId == item.MyShowsId);
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
                for(int i = 0; i < item.GenreIds.Count(); i++)
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
                        RussianName=item.Country.RussianName
                    };
                    await db.AddAsync(country);
                    await db.SaveChangesAsync();
                }
                series.Country = country;
                series.Raiting = item.Raiting;
                //using (WebClient webClient = new WebClient())
                //{
                //    Debug.WriteLine(series.Title);
                //    foreach (var episode in item.Episodes)
                //    {
                //        if (episode.PicturePath.Length > 0)
                //        {
                //            string EpisodePicturePath = Path.Combine(webRoot, GetSafeFilename(series.OriginalTitle), episode.SeasonNumber.ToString());
                //            if (!System.IO.File.Exists(EpisodePicturePath))
                //            {
                //                Directory.CreateDirectory(EpisodePicturePath);
                //            }
                //            EpisodePicturePath = Path.Combine(EpisodePicturePath, Path.GetFileName(episode.PicturePath));
                //            try
                //            {
                //                webClient.DownloadFile(episode.PicturePath, EpisodePicturePath);
                //            }
                //            catch (WebException wex)
                //            {
                //                if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotFound)
                //                {

                //                }
                //            }
                //            episode.PicturePath = EpisodePicturePath;
                //        }
                //    }
                //}
                series.Episodes = item.Episodes;
                Series.Add(series);
            }
            db.UpdateRange(Series);
            await db.SaveChangesAsync();
            return "Список жанров получен";
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