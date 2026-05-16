using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;

namespace SchoolProject.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class AdminDashboardController : Controller
    {
        private readonly AppDbContext _context;

        public AdminDashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /AdminDashboard
        [HttpGet]
        [Route("AdminDashboard")]
        [Route("Admin")]
        public IActionResult Index()
        {
            // Stats
            ViewBag.TotalSchools = _context.Schools.Count(s => s.IsActive);
            ViewBag.TotalColleges = _context.Colleges.Count(c => c.IsActive);

            // Count enquiries from BOTH tables
            var schoolEnquiryCount = _context.Enquiries.Count();      // response table
            var collegeEnquiryCount = _context.CollegeEnquiries.Count(); // tb_college_enquiries
            ViewBag.TotalEnquiries = schoolEnquiryCount + collegeEnquiryCount;
            ViewBag.SchoolEnquiryCount = schoolEnquiryCount;
            ViewBag.CollegeEnquiryCount = collegeEnquiryCount;

            ViewBag.TotalBlogPosts = _context.BlogPosts.Count();

            // Recent enquiries (limited to 5 each to avoid timeout)
            ViewBag.RecentSchoolEnquiries = _context.Enquiries
                .OrderByDescending(e => e.EntryDate)
                .Take(5)
                .ToList();

            ViewBag.RecentCollegeEnquiries = _context.CollegeEnquiries
                .Include(e => e.College)
                .OrderByDescending(e => e.EntryDate)
                .Take(5)
                .ToList();

            return View();
        }
    }
}