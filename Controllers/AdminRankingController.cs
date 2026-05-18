using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models;

namespace SchoolProject.Controllers
{
    // ─────────────────────────────────────────────
    // DTOs
    // ─────────────────────────────────────────────
    public class RankUpdateDto
    {
        public int InstituteId { get; set; }
        public int ListingRank { get; set; }
        public bool IsSponsored { get; set; }
    }

    public class BulkRankDto
    {
        public List<RankUpdateDto> Schools { get; set; } = new();
    }

    // ─────────────────────────────────────────────
    // Controller
    // ─────────────────────────────────────────────
    [Route("admin/ranking")]
    public class AdminRankingController : Controller
    {
        private readonly AppDbContext _context;

        public AdminRankingController(AppDbContext context)
        {
            _context = context;
        }

        // ── GET /admin/ranking ──────────────────────────────────────
        // Shows the drag-and-drop ranking admin UI
        [HttpGet("")]
        public IActionResult Index(int? cityId, int? syllabusId)
        {
            var cities = _context.Cities
                .OrderBy(c => c.CityName)
                .ToList();

            var syllabuses = _context.Syllabuses
                .OrderBy(s => s.SyllabusName)
                .ToList();

            ViewBag.Cities = cities;
            ViewBag.Syllabuses = syllabuses;
            ViewBag.SelectedCityId = cityId;
            ViewBag.SelectedSyllabusId = syllabusId;

            List<School> schools = new();

            if (cityId.HasValue && syllabusId.HasValue)
            {
                schools = _context.Schools
                    .Include(s => s.City)
                    .Where(s =>
                        s.CityId == cityId &&
                        s.SchoolSyllabuses!.Any(ss => ss.SyllabusId == syllabusId))
                    .OrderBy(s =>
                        s.ListingRank == null || s.ListingRank == 0 ? 999999 : s.ListingRank)
                    .ThenBy(s => s.InstituteName)
                    .ToList();
            }

            return View("~/Views/Admin/Ranking.cshtml", schools);
        }

        // ── POST /admin/ranking/update ──────────────────────────────
        // Single school rank update (called on inline edit)
        [HttpPost("update")]
        public IActionResult Update([FromBody] RankUpdateDto dto)
        {
            var school = _context.Schools.Find(dto.InstituteId);
            if (school == null)
                return NotFound(new { success = false, message = "School not found" });

            school.ListingRank = dto.ListingRank;
            school.IsSponsored = dto.IsSponsored;
            _context.SaveChanges();

            return Ok(new { success = true });
        }

        // ── POST /admin/ranking/bulk-update ────────────────────────
        // Saves the full reordered list after drag-and-drop
        [HttpPost("bulk-update")]
        public IActionResult BulkUpdate([FromBody] BulkRankDto dto)
        {
            if (dto?.Schools == null || !dto.Schools.Any())
                return BadRequest(new { success = false, message = "No data" });

            var ids = dto.Schools.Select(s => s.InstituteId).ToList();
            var schools = _context.Schools
                .Where(s => ids.Contains(s.InstituteId))
                .ToList();

            foreach (var update in dto.Schools)
            {
                var school = schools.FirstOrDefault(s => s.InstituteId == update.InstituteId);
                if (school == null) continue;
                school.ListingRank = update.ListingRank;
                school.IsSponsored = update.IsSponsored;
            }

            _context.SaveChanges();
            return Ok(new { success = true, updated = schools.Count });
        }

        // ── POST /admin/ranking/reset ───────────────────────────────
        // Resets all ranks for a city+syllabus combo to 0
        [HttpPost("reset")]
        public IActionResult Reset([FromBody] ResetDto dto)
        {
            var schools = _context.Schools
                .Where(s =>
                    s.CityId == dto.CityId &&
                    s.SchoolSyllabuses!.Any(ss => ss.SyllabusId == dto.SyllabusId))
                .ToList();

            foreach (var s in schools)
            {
                s.ListingRank = 0;
                s.IsSponsored = false;
            }

            _context.SaveChanges();
            return Ok(new { success = true, reset = schools.Count });
        }
    }

    public class ResetDto
    {
        public int CityId { get; set; }
        public int SyllabusId { get; set; }
    }
}