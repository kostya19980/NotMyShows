using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;
using Microsoft.AspNetCore.Identity;
using NotMyShows.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using NotMyShows.Data;
using NotMyShows.Services;
using Microsoft.AspNetCore.Http;

namespace NotMyShows.Controllers
{
    public class AccountController : Controller
    {
        private IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
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
                var result = await _userService.LoginUserAsync(model, false);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                if (result.IsSuccess)
                {
                    // проверяем, принадлежит ли URL приложению
                    if (!string.IsNullOrEmpty(model.ReturnURL) && Url.IsLocalUrl(model.ReturnURL))
                    {
                        return Redirect(model.ReturnURL);
                    }
                    else
                    {
                        return RedirectToAction("Profile", "Profiles", new { id = result.UserProfileId });
                    }
                }
                ModelState.AddModelError("ModelOnly", result.Message);
            }
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
                var result = await _userService.RegisterUserAsync(model, false);
                if (result == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                if (result.IsSuccess)
                {
                    return RedirectToAction("Profile", "Profiles", new { id = result.UserProfileId });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("ModelOnly", error);
                }
            }
            return View(model);
            //if (ModelState.IsValid)
            //{
            //    string Name = CreateUserName(model.Email);
            //    User user = new User 
            //    { 
            //        Email = model.Email, 
            //        UserName = Name,
            //        UserProfile = await CreateUserProfile()
            //    };
            //    // добавляем пользователя
            //    var result = await _userManager.CreateAsync(user, model.Password);
            //    if (result.Succeeded)
            //    {
            //        // установка куки
            //        await _signInManager.SignInAsync(user, false);
            //        return RedirectToAction("Profile", "Profiles", new { id = user.UserProfile.Id });
            //    }
            //    else
            //    {
            //        foreach (var error in result.Errors)
            //        {
            //            ModelState.AddModelError("ModelOnly", error.Description);
            //        }
            //    }
            //}
            //return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
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
