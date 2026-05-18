using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Search;

namespace SchoolProject.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string q)
        {
            var results = new List<SearchResultViewModel>();

            if (string.IsNullOrWhiteSpace(q))
                return View(results);

            q = q.Trim();

            var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Schools
            var schools = _context.Schools
                .Include(s => s.City)
                .Where(s =>
                    words.All(w =>
                        EF.Functions.Like(s.InstituteName, "%" + w + "%") ||
                        (s.Keyword != null && EF.Functions.Like(s.Keyword, "%" + w + "%")) ||
                        (s.City.CityName != null && EF.Functions.Like(s.City.CityName, "%" + w + "%"))
                    )
                )
                .Select(s => new SearchResultViewModel
                {
                    Title = s.InstituteName,
                    Url = "/school/" + s.InstituteSlug,
                    Type = "School",
                    Description = s.Address
                })
                .Take(20)
                .ToList();

            results.AddRange(schools);

            // Colleges
            var colleges = _context.Colleges
                .Where(c =>
                    words.All(w =>
                        EF.Functions.Like(c.InstituteName, "%" + w + "%") ||
                        (c.Address != null && EF.Functions.Like(c.Address, "%" + w + "%"))
                    )
                )
                .Select(c => new SearchResultViewModel
                {
                    Title = c.InstituteName,
                    Url = "/college/" + c.InstituteSlug,
                    Type = "College",
                    Description = c.Address
                })
                .Take(20)
                .ToList();

            results.AddRange(colleges);

            // Courses
            var courses = _context.Courses
                .Where(c =>
                    words.All(w =>
                        EF.Functions.Like(c.CourseName, "%" + w + "%") ||
                        (c.ShortName != null && EF.Functions.Like(c.ShortName, "%" + w + "%"))
                    )
                )
                .Select(c => new SearchResultViewModel
                {
                    Title = c.CourseName,
                    Url = "/courses/" + c.CourseSlug,
                    Type = "Course",
                    Description = c.ShortName
                })
                .Take(20)
                .ToList();

            results.AddRange(courses);

            results = results
                .OrderBy(r => r.Type)
                .ThenBy(r => r.Title)
                .ToList();

            ViewBag.Query = q;

            return View(results);
        }

        [HttpGet]
        public IActionResult AutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            term = term.Trim();

            var schoolResults = _context.Schools
                .Where(s =>
                    EF.Functions.Like(s.InstituteName, "%" + term + "%")
                )
                .Select(s => new
                {
                    label = s.InstituteName + " (School)",
                    value = s.InstituteName,
                    url = "/school/" + s.InstituteSlug
                })
                .Take(5)
                .ToList();

            var collegeResults = _context.Colleges
                .Where(c =>
                    EF.Functions.Like(c.InstituteName, "%" + term + "%")
                )
                .Select(c => new
                {
                    label = c.InstituteName + " (College)",
                    value = c.InstituteName,
                    url = "/college/" + c.InstituteSlug
                })
                .Take(5)
                .ToList();

            var courseResults = _context.Courses
                .Where(c =>
                    EF.Functions.Like(c.CourseName, "%" + term + "%")
                )
                .Select(c => new
                {
                    label = c.CourseName + " (Course)",
                    value = c.CourseName,
                    url = "/courses/" + c.CourseSlug
                })
                .Take(5)
                .ToList();

            var results = schoolResults
                .Concat(collegeResults)
                .Concat(courseResults)
                .Take(10)
                .ToList();

            return Json(results);
        }
    }
}
