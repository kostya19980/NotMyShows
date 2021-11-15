﻿using Microsoft.AspNetCore.Mvc;
using NotMyShows.Models;

namespace PrintStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly SeriesContext db;
        public AccountController(SeriesContext context)
        {
            db = context;
        }
        //[HttpGet]
        //public IActionResult Login()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(LoginModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        User user = await db.User.Include(u => u.Role).FirstOrDefaultAsync(u => u.Login == model.Login && u.Password == model.Password);
        //        if (user != null)
        //        {
        //            await Authenticate(user,model.RememberMe); // аутентификация
        //            return Json(new { success = "true" });
        //        }
        //        else
        //        {
        //            return Json(new {error="Неверный логин и(или) пароль!" });
        //        }
        //    }
        //        return Json("model_error");
        //}
        //[HttpGet]
        //public IActionResult Register()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Register(RegisterModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        Role userRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "user");
        //        User user = await db.User.FirstOrDefaultAsync(u => u.Login == model.Login || u.Email == model.Email);
        //        if (user == null)
        //        {
        //            user = new User { Email = model.Email, Login = model.Login, Password = model.Password, Role = userRole };
        //            await db.User.AddAsync(user);
        //        }
        //        else
        //        {
        //            if (user.RoleId == 3)
        //            {
        //                user.Role = userRole;
        //                user.Login = model.Login;
        //                user.Email = model.Email;
        //                user.Password = model.Password;
        //                db.User.Update(user);
        //            }
        //        }
        //        await db.SaveChangesAsync();
        //        await Authenticate(user, false); // аутентификация
        //        return Json(new { success = "true" });
        //    }
        //    return Json("model_error");
        //}
        //public async Task<IActionResult> CheckPhone(string Login)
        //{
        //    User user = await db.User.FirstOrDefaultAsync(u => u.Login == Login);
        //    if (user == null || user.RoleId == 3 || User.Identity.Name == Login)
        //    {
        //        return Json(true);
        //    }
        //    return Json(false);
        //}
        //public async Task<IActionResult> CheckEmail(string Email)
        //{
        //    User user = await db.User.FirstOrDefaultAsync(u => u.Email == Email);
        //    if (user == null || user.RoleId==3)
        //    {
        //        return Json(true);
        //    }
        //    if (User.Identity.IsAuthenticated)
        //    {
        //        string UserName = User.Identity.Name;
        //        User CurrentUser = await db.User.FirstOrDefaultAsync(u => u.Login == UserName);
        //        if(CurrentUser.Email == Email)
        //        {
        //            return Json(true);
        //        }
        //    }
        //    return Json(false);
        //}
        //private async Task Authenticate(User user,bool RememberMe)
        //{
        //    // создаем claim (User.Identity.)
        //    var claims = new List<Claim> 
        //    {
        //        new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
        //        new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role?.Name)
        //    };
        //    // создаем объект ClaimsIdentity
        //    ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType,
        //        ClaimsIdentity.DefaultRoleClaimType);
        //    //Запомнить меня
        //    var properties = new AuthenticationProperties();
        //    properties.IsPersistent = RememberMe;
        //    // установка аутентификационных куки
        //    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id), properties);
        //}

        //[Authorize(Roles = "admin,user")]
        //public async Task<IActionResult> Logout()
        //{
        //    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        //    HttpContext.Session.SetString("UserName", "");
        //    return Json("success");
        //}
    }
}