using IdentityModel;
using IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NotMyShows.Models;
using System;
using System.Net;

namespace NotMyShows
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            const string oidc = OpenIdConnectDefaults.AuthenticationScheme;
            services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = oidc;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddOpenIdConnect(oidc, config =>
            {
                config.ClientId = "NotMyShows";
                config.ClientSecret = "NotMyShowsSecret";
                config.SaveTokens = true;
                config.Authority = "https://localhost:9001";
                config.ResponseType = "code";
                config.GetClaimsFromUserInfoEndpoint = true;
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddAuthorization();
            services.AddSession();
            services.AddHttpClient();
            string connection = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SeriesContext>(options => options.UseSqlServer(connection));
            services.AddHttpContextAccessor();
            services.AddMvc().AddNewtonsoftJson(option => {
                option.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;})
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
