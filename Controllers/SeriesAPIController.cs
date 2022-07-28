using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotMyShows.Data;
using NotMyShows.Models;
using NotMyShows.Services;
using NotMyShows.ViewModel;

namespace NotMyShows.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SeriesAPIController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesAPIController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        // GET: api/SeriesAPI
        [AllowAnonymous]
        public async Task<SeriesCatalogViewModel> GetSeriesListAsync(int startIndex, int count)
        {
            return await _seriesService.GetSeriesListAsync(startIndex, count);
        }
        [AllowAnonymous]
        public async Task<SeriesViewModel> GetSeriesAsync(int seriesId)
        {
            return await _seriesService.GetSeriesAsync(seriesId);
        }
        [AllowAnonymous]
        public async Task<EpisodeViewModel> GetEpisodeAsync(int EpisodeId)
        {
            return await _seriesService.GetEpisodeAsync(EpisodeId);
        }

    }
}
