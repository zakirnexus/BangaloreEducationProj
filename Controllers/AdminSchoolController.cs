using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models;
using System.Linq;

namespace SchoolProject.Controllers
{
    public class AdminSchoolController : Controller
    {
        private readonly AppDbContext _context;

        public AdminSchoolController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LIST
        public IActionResult Index()
        {
            var schools = _context.Schools
                .Include(s => s.City)
                .Include(s => s.Syllabus)
                .OrderByDescending(s => s.InstituteId)
                .ToList();

            return View(schools);
        }

        // ✅ CREATE (GET)
        public IActionResult Create()
        {
            ViewBag.Cities = _context.Cities.ToList();
            ViewBag.Syllabuses = _context.Syllabuses.ToList();
            return View();
        }

        // ✅ CREATE (POST)
        [HttpPost]
        public IActionResult Create(School model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = _context.Cities.ToList();
                ViewBag.Syllabuses = _context.Syllabuses.ToList();
                return View(model);
            }

            _context.Schools.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ EDIT (GET)
        public IActionResult Edit(int id)
        {
            var school = _context.Schools.Find(id);
            if (school == null) return NotFound();

            ViewBag.Cities = _context.Cities.ToList();
            ViewBag.Syllabuses = _context.Syllabuses.ToList();

            return View(school);
        }

        // ✅ EDIT (POST)
        [HttpPost]
        public IActionResult Edit(School model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = _context.Cities.ToList();
                ViewBag.Syllabuses = _context.Syllabuses.ToList();
                return View(model);
            }

            _context.Schools.Update(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ✅ DELETE
        public IActionResult Delete(int id)
        {
            var school = _context.Schools.Find(id);
            if (school == null) return NotFound();

            _context.Schools.Remove(school);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}