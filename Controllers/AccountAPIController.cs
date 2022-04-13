using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.Services;
using NotMyShows.ViewModel;


namespace NotMyShows.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountAPIController : ControllerBase
    {
        private IUserService _userService;
        public AccountAPIController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterUserAsync(model, true);
                return GetResponse(result);
            }
            return BadRequest("Введённая вами комбинация логина и пароля не верна");
        }
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(model, true);
                return GetResponse(result);
            }
            return BadRequest("Введённая вами комбинация логина и пароля не верна");
        }
        public IActionResult GetResponse(UserManagerResponse result)
        {
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
