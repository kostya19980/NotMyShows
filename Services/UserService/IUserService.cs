using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.ViewModel;
using System;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NotMyShows.Services
{
    public interface IUserService
    {
        Task<UserManagerResponse> RegisterUserAsync(RegisterModel model, bool isApi);
        Task<UserManagerResponse> LoginUserAsync(LoginModel model, bool isApi);
        Task LogoutAsync();
        Task<string> GetUserNameByIdAsync(int Id);
        Task<int> GetUserProfileIdAsync(string userId);
        string GetUserName();
        string GetUserId();
        bool IsAuthenticated();
    }
    public class UserService: IUserService
    {
        readonly SeriesContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(SeriesContext context, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<UserManagerResponse> LoginUserAsync(LoginModel model, bool isApi)
        {
            User user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new UserManagerResponse
                {
                    Message = "Введённая вами комбинация логина и пароля не найдена",
                    IsSuccess = false,
                };
            }
            //JWT token
            if (isApi)
            {
                bool result = await _userManager.CheckPasswordAsync(user, model.Password);
                if (result)
                {
                    Claim[] claims = new[]
                    {
                        new Claim("Email", model.Email),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                    };
                    JwtSecurityToken token = CreateJwtToken(claims);
                    return new UserManagerResponse
                    {
                        Message = new JwtSecurityTokenHandler().WriteToken(token),
                        IsSuccess = true,
                        ExpireDate = token.ValidTo,
                        UserProfileId = await GetUserProfileIdAsync(user.Id),
                    };
                }
            }
            else
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (signInResult.Succeeded)
                {
                    return new UserManagerResponse
                    {
                        UserProfileId = await GetUserProfileIdAsync(user.Id),
                        Message = "Вход был успешно осуществлен",
                        IsSuccess = true
                    };
                }
            }
            return new UserManagerResponse
            {
                Message = "Введённая вами комбинация логина и пароля не верна",
                IsSuccess = false,
            };
        }
        public string GetUserId()
        {
            return _httpContextAccessor.HttpContext.User.GetSub();
        }
        public async Task<string> GetUserNameByIdAsync(int Id)
        {
            var profile = await _context.UserProfiles.Select(p => new { Id = p.Id, Name = p.User.UserName }).AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
            return profile.Name;
        }
        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext.User.Identity.Name;
        }
        public JwtSecurityToken CreateJwtToken(Claim[] claims)
        {
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["AuthSettings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return token;
        }
        public async Task<UserManagerResponse> RegisterUserAsync(RegisterModel model, bool isApi)
        {
            if (model == null) 
            {
                return null;
            }
            string Name = CreateUserName(model.Email);
            User user = new User
            {
                Email = model.Email,
                UserName = Name,
                UserProfile = await CreateUserProfile()
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!isApi)
                {
                    await _signInManager.SignInAsync(user, false);
                }
                return new UserManagerResponse
                {
                    UserProfileId = user.UserProfile.Id,
                    Message = "Пользователь создан успешно!",
                    IsSuccess = true,
                };
            }
            return new UserManagerResponse
            {
                Message = "Пользователь не был создан",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }
        public async Task LogoutAsync()
        {
            // удаляем аутентификационные куки
            await _signInManager.SignOutAsync();
        }
        public async Task<int> GetUserProfileIdAsync(string userId)
        {
            UserProfile userProfile = await _context.UserProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
            return userProfile.Id;
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
            var statuses = await _context.WatchStatuses.ToListAsync();
            if (statuses.Count == 0)
            {
                string[] names = { "Смотрю", "Запланировано", "Отложено", "Брошено", "Просмотрено", "Не смотрю" };
                foreach (var name in names)
                {
                    WatchStatus status = new WatchStatus
                    {
                        StatusName = name
                    };
                    await _context.AddAsync(status);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
