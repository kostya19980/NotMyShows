using NotMyShows.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NotMyShows.Data;
using NotMyShows.Services;
using Microsoft.AspNetCore.Authorization;

namespace NotMyShows
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SeriesContext>(options => options.UseSqlServer(connection));
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            }).AddEntityFrameworkStores<SeriesContext>()
            .AddDefaultTokenProviders();
            services.AddAuthentication()
              .AddCookie(cfg => cfg.SlidingExpiration = true)
              .AddJwtBearer(options => 
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidateAudience = true,
                      ValidAudience = Configuration["AuthSettings:Audience"],
                      ValidIssuer = Configuration["AuthSettings:Issuer"],
                      RequireExpirationTime = true,
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AuthSettings:Key"])),
                      ValidateIssuerSigningKey = true,
                  };
              });
            services.AddScoped<IUserService, UserService>();
            //const string oidc = OpenIdConnectDefaults.AuthenticationScheme;
            //services.AddAuthentication(config =>
            //{
            //    config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    config.DefaultChallengeScheme = oidc;
            //})
            //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            //.AddOpenIdConnect(oidc, config =>
            //{
            //    config.ClientId = "NotMyShows";
            //    config.ClientSecret = "NotMyShowsSecret";
            //    config.SaveTokens = true;
            //    config.Authority = "https://localhost:9001";
            //    config.ResponseType = "code";
            //    config.GetClaimsFromUserInfoEndpoint = true;
            //});
            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});
            //services.AddAuthorization();
            //services.AddSession();
            //services.AddHttpClient();
            //services.AddHttpContextAccessor();
            services.AddMvc().AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddRazorRuntimeCompilation();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
