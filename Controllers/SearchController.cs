using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models;
using System.Linq;

namespace SchoolProject.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // 🔍 MAIN SEARCH PAGE
        public IActionResult Index(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new List<School>());

            q = q.ToLower().Trim();

            var words = q.Split(' ');

            var results = _context.Schools
                .Include(s => s.City)
                .Where(s =>
                    words.All(w =>
                        s.InstituteName.ToLower().Contains(w) ||
                        (s.Keyword != null && s.Keyword.ToLower().Contains(w)) ||
                        (s.City.CityName != null && s.City.CityName.ToLower().Contains(w))
                    )
                )
                .ToList();

            return View(results); // ✅ FIXED
        }

        // ⚡ AUTOCOMPLETE API
        [HttpGet]
        public IActionResult AutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            term = term.ToLower();

            var results = _context.Schools
                .Where(s =>
                    s.InstituteName.ToLower().Contains(term) ||
                    (s.Keyword != null && s.Keyword.ToLower().Contains(term))
                )
                .Select(s => new
                {
                    label = s.InstituteName,
                    value = s.InstituteName,
                    url = "/school/" + s.InstituteSlug
                })
                .Take(10)
                .ToList();

            return Json(results);
        }
    }
}