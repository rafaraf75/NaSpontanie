using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaSpontanie.API.Data;
using NaSpontanie.API.Models;
using NaSpontanie.API.Dtos.Reports;



namespace NaSpontanie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Report>>> GetReports()
        {
            return await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportedEvent)
                .ToListAsync();
        }

        // GET: api/Reports/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Report>> GetReport(int id)
        {
            var report = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportedEvent)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            return report;
        }

        // POST: api/Reports
        [HttpPost]
        public async Task<ActionResult<Report>> PostReport(CreateReportDto dto)
        {
            var reporter = await _context.Users.FindAsync(dto.ReporterId);
            if (reporter == null)
                return BadRequest("Reporter not found.");

            var report = new Report
            {
                ReporterId = dto.ReporterId,
                ReportedUserId = dto.ReportedUserId,
                ReportedEventId = dto.ReportedEventId,
                Reason = dto.Reason,
                CreatedAt = DateTime.Now,
                Reporter = reporter
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
        }


        // PUT: api/Reports/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReport(int id, Report report)
        {
            if (id != report.Id)
            {
                return BadRequest();
            }

            _context.Entry(report).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(id))
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

        // DELETE: api/Reports/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
