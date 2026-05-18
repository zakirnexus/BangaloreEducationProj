using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using SchoolProject.Models.Colleges;
using SchoolProject.Models.Courses;
using System;
using System.Linq;


namespace SchoolProject.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class AdminCollegeController : Controller
    {
        private readonly AppDbContext _context;
        private const int BENGALURU_CITY_ID = 7281;

        public AdminCollegeController(AppDbContext context)
        {
            _context = context;
        }

        // ===== LIST ALL COLLEGES =====
        public IActionResult Index(string search = "", int? cityId = null)
        {
            var query = _context.Colleges
                .Include(c => c.City)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(c =>
                    c.InstituteName.ToLower().Contains(searchLower) ||
                    (c.Keywords != null && c.Keywords.ToLower().Contains(searchLower)) ||
                    (c.Address != null && c.Address.ToLower().Contains(searchLower)) ||
                    (c.AboutInstitute != null && c.AboutInstitute.ToLower().Contains(searchLower)) ||
                    (c.City != null && c.City.CityName != null && c.City.CityName.ToLower().Contains(searchLower)));
            }

            if (cityId.HasValue)
                query = query.Where(c => c.CityId == cityId);

            var colleges = query
                .OrderByDescending(c => c.CreatedDate)
                .Take(500)
                .ToList();

            ViewBag.Cities = _context.Cities.Where(c => c.IsActive).ToList();
            return View(colleges);
        }

        // ===== CREATE (GET) =====
        public IActionResult Create()
        {
            var model = new College
            {
                CityId = BENGALURU_CITY_ID,  // Bengaluru pre-selected
                IsActive = true,
                CreatedDate = DateTime.Now
            };
            ViewBag.Cities = _context.Cities.Where(c => c.IsActive).ToList();
            ViewBag.States = _context.States.Where(s => s.IsActive).ToList();
            ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
            ViewBag.Coeds = _context.Coeds.ToList();
            ViewBag.Ownerships = _context.InstOwnerships.ToList();
            return View();
        }

        // ===== CREATE (POST) =====
        [HttpPost]
        public IActionResult Create(College model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = _context.Cities.Where(c => c.IsActive).ToList();
                return View(model);
            }

            model.InstituteSlug = GenerateSlug(model.InstituteName);
            model.CreatedDate = DateTime.Now;
            model.ModifiedDate = DateTime.Now;

            _context.Colleges.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ===== EDIT (GET) =====
        public IActionResult Edit(int id)
        {
            var college = _context.Colleges.Find(id);
            if (college == null) return NotFound();
            
            ViewBag.Cities = _context.Cities.Where(c => c.IsActive).ToList();
            ViewBag.States = _context.States.Where(s => s.IsActive).ToList();
            ViewBag.Localities = _context.Localities.Where(l => l.IsActive).ToList();
            ViewBag.Coeds = _context.Coeds.ToList();
            ViewBag.Ownerships = _context.InstOwnerships.ToList();

            return View(college);
        }

        // ===== EDIT (POST) =====
        [HttpPost]
        public IActionResult Edit(College model)
        {
            var existing = _context.Colleges.Find(model.InstituteId);
            if (existing == null) return NotFound();

            existing.InstituteName = model.InstituteName;
            existing.Address = model.Address;
            existing.LocalityId = model.LocalityId;
            existing.CityId = model.CityId;
            existing.StateId = model.StateId;
            existing.Pincode = model.Pincode;
            existing.Telephone = model.Telephone;
            existing.Mobile = model.Mobile;
            existing.Email = model.Email;
            existing.Email2 = model.Email2;
            existing.Website = model.Website;
            existing.Estd = model.Estd;
            existing.Accreditation = model.Accreditation;
            existing.NaacGrade = model.NaacGrade;
            existing.NbaAccredited = model.NbaAccredited;
            existing.ApprovedBy = model.ApprovedBy;
            existing.AffiliatedTo = model.AffiliatedTo;
            existing.CoedId = model.CoedId;
            existing.InstOwnershipId = model.InstOwnershipId;
            existing.HostelAvailable = model.HostelAvailable;
            existing.TransportAvailable = model.TransportAvailable;
            existing.LibraryAvailable = model.LibraryAvailable;
            existing.SportsFacilities = model.SportsFacilities;
            existing.WifiCampus = model.WifiCampus;
            existing.PlacementPercentage = model.PlacementPercentage;
            existing.AvgPackageLpa = model.AvgPackageLpa;
            existing.ScholarshipAvailable = model.ScholarshipAvailable;
            existing.ScholarshipDetails = model.ScholarshipDetails;
            existing.MetaTitle = model.MetaTitle;
            existing.MetaDescription = model.MetaDescription;
            existing.Keywords = model.Keywords;
            existing.AboutInstitute = model.AboutInstitute;
            existing.ListingRank = model.ListingRank;
            existing.IsSponsored = model.IsSponsored;
            existing.IsFeatured = model.IsFeatured;
            existing.IsActive = model.IsActive;
            existing.ModifiedDate = DateTime.Now;

            // Regenerate slug if name changed
            if (existing.InstituteName != model.InstituteName)
                existing.InstituteSlug = GenerateSlug(model.InstituteName);

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // ===== DELETE =====
        public IActionResult Delete(int id)
        {
            var college = _context.Colleges.Find(id);
            if (college != null)
            {
                // Soft delete
                college.IsActive = false;
                college.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // ===== MANAGE COURSES FOR A COLLEGE =====
        public IActionResult Courses(int collegeId)
        {
            var college = _context.Colleges
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.Course)
                .FirstOrDefault(c => c.InstituteId == collegeId);

            if (college == null) return NotFound();

            ViewBag.AllCourses = _context.Courses
                .Where(c => c.IsActive)
                .OrderBy(c => c.CourseName)
                .ToList();

            // NEW: Load all specializations grouped by course for dropdown
            ViewBag.Specializations = _context.Specializations
                .Where(s => s.IsActive)
                .OrderBy(s => s.CourseId)
                .ThenBy(s => s.SpecializationName)
                .ToList();

            return View(college);
        }

        [HttpPost]
        public IActionResult AddCourse(int instituteId, int courseId, int? specializationId,
        string? feesStructure, string? entranceExam, string? courseDuration)
        {
            var cc = new CollegeCourse
            {
                InstituteId = instituteId,
                CourseId = courseId,
                SpecializationId = specializationId,  // Store ID instead of text
                FeesStructure = feesStructure,
                EntranceExam = entranceExam,
                CourseDuration = courseDuration,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            _context.CollegeCourses.Add(cc);
            _context.SaveChanges();

            TempData["Success"] = "Course added successfully.";
            return RedirectToAction("Courses", new { collegeId = instituteId });
        }

        [HttpPost]
public IActionResult RemoveCourse(int collegeCourseId)
{
    var cc = _context.CollegeCourses.Find(collegeCourseId);
    if (cc != null)
    {
        cc.IsActive = false;
        cc.ModifiedDate = DateTime.Now;
        _context.SaveChanges();
        
        TempData["Success"] = "Course removed successfully.";
        return RedirectToAction("Courses", new { collegeId = cc.InstituteId });
    }
    
    TempData["Error"] = "Course not found.";
    return RedirectToAction("Index");
}

        // ===== ENQUIRIES =====
        public IActionResult Enquiries(string college = "", string fromDate = "", string toDate = "")
        {
            var query = _context.CollegeEnquiries
                .Include(e => e.College)
                .AsQueryable();

            if (!string.IsNullOrEmpty(college))
                query = query.Where(e => e.College!.InstituteName.Contains(college));

            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
                query = query.Where(e => e.EntryDate >= from);

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
                query = query.Where(e => e.EntryDate <= to);

            var enquiries = query
                .OrderByDescending(e => e.EntryDate)
                .Take(500)
                .ToList();

            return View(enquiries);
        }

        // ===== SLUG GENERATOR =====
        private string GenerateSlug(string? title)
        {
            if (string.IsNullOrEmpty(title)) return "";
            return title.ToLower()
                .Replace(" ", "-")
                .Replace(",", "")
                .Replace(".", "")
                .Replace("/", "")
                .Replace("--", "-");
        }
    }
}
