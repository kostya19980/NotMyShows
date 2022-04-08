using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using Microsoft.AspNetCore.Identity;
using NotMyShows.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace NotMyShows.Controllers
{
    public class AccountController : Controller
    {
        readonly SeriesContext db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(SeriesContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            db = context;
        }
        public IActionResult Login(string returnURL = null)
        {
            var model = new LoginModel
            {
                ReturnURL = returnURL
            };
            return View(model);
            //return RedirectToAction("Register", "Account", returnURL);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("ModelOnly", "Введённая вами комбинация логина и пароля не найдена");
                    return View(model);
                    //return Json(new { error = "Неверный логин и(или) пароль!" });
                }
                else
                {
                    var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                    if (signInResult.Succeeded)
                    {
                        // проверяем, принадлежит ли URL приложению
                        if (!string.IsNullOrEmpty(model.ReturnURL) && Url.IsLocalUrl(model.ReturnURL))
                        {
                            return Redirect(model.ReturnURL);
                        }
                        else
                        {
                            UserProfile userProfile = await db.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == user.Id);
                            return RedirectToAction("Profile", "Profiles", new { id = userProfile.Id });
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("ModelOnly", "Введённая вами комбинация логина и пароля не верна");
                    }
                }
            }
            ModelState.AddModelError("ModelOnly", "Неизвестная ошибка!");
            return View(model);
        }
        [HttpGet]
        public IActionResult Register(string returnURL)
        {
            var model = new RegisterModel
            {
                ReturnURL = returnURL
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                string Name = CreateUserName(model.Email);
                User user = new User 
                { 
                    Email = model.Email, 
                    UserName = Name,
                    UserProfile = await CreateUserProfile()
                };
                // добавляем пользователя
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // установка куки
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Profile", "Profiles", new { id = user.UserProfile.Id });
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("ModelOnly", error.Description);
                    }
                }
            }
            ModelState.AddModelError("ModelOnly", "Неизвестная ошибка!");
            return View(model);
        }
        public async Task<UserProfile> CreateUserProfile()
        {
            await CreateWatchStatuses();
            UserProfile userProfile = new UserProfile
            {
                ImageSrc = Path.Combine("images", "UserAvatars", "DefaultAvatar.png")
            };
            return userProfile;
        }
        public string CreateUserName(string Email)
        {
            string Name = Email;
            int pos = Name.LastIndexOf("@");
            Name = Name.Remove(pos, Name.Length - pos);
            return Name;
        }
        public async Task CreateWatchStatuses()
        {
            var statuses = await db.WatchStatuses.ToListAsync();
            if (statuses.Count == 0)
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
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        //public async Task<IActionResult> LogoutAsync(string logoutId)
        //{
        //    await _signInManager.SignOutAsync();
        //    var logoutResult = await _interactionService.GetLogoutContextAsync(logoutId);
        //    if (string.IsNullOrEmpty(logoutResult.PostLogoutRedirectUri))
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //    return Redirect(logoutResult.PostLogoutRedirectUri);
        //}
    }
}
