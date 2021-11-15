using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NotMyShows.Models;

namespace NotMyShows.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SeriesAPIController : ControllerBase
    {
        private readonly SeriesContext _context;

        public SeriesAPIController(SeriesContext context)
        {
            _context = context;
        }

        // GET: api/SeriesAPI
        [HttpGet]
        public async Task<IEnumerable<Series>> GetSeries()
        {
            var series = await _context.Series.Include(s=>s.Status).Include(ch=>ch.Channel)
                .Where(x => x.Id >= 1 && x.Id <= 100)
                .Select(s => new Series
                {
                    Id = s.Id,
                    Title =s.Title,
                    OriginalTitle=s.OriginalTitle,
                    Raiting=s.Raiting,
                    Status = s.Status,
                    Channel=s.Channel,
                    Viewers =s.Viewers,
                    PicturePath = s.PicturePath

                }).ToListAsync();
            return series;
        }

        // GET: api/SeriesAPI/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Series>> GetSeriesById(int id)
        {
            var series = await _context.Series.Include(e=>e.Episodes).Include(r => r.Raiting).Include(s => s.Status).Include(ch => ch.Channel).Include(c => c.Country)
                .Include(rev => rev.Reviews).Include(sg => sg.SeriesGenres).ThenInclude(g => g.Genre).FirstOrDefaultAsync(x=>x.Id==id);

            if (series == null)
            {
                return NotFound();
            }

            return series;
        }
        [HttpPost]
        public async Task<string> AddSeriesReview(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return "Success";
        }
        // PUT: api/SeriesAPI/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSeries(int id, Series series)
        {
            if (id != series.Id)
            {
                return BadRequest();
            }

            _context.Entry(series).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeriesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SeriesAPI
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Series>> PostSeries(Series series)
        {
            _context.Series.Add(series);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSeries", new { id = series.Id }, series);
        }

        // DELETE: api/SeriesAPI/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Series>> DeleteSeries(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null)
            {
                return NotFound();
            }

            _context.Series.Remove(series);
            await _context.SaveChangesAsync();

            return series;
        }

        private bool SeriesExists(int id)
        {
            return _context.Series.Any(e => e.Id == id);
        }
    }
}
