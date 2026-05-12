using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Data;
using System;
using System.Linq;

namespace SchoolProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ ENQUIRY LIST PAGE
        public IActionResult Enquiries(string school = "", string fromDate = "", string toDate = "")
        {
            var query = _context.Enquiries
                .Include(e => e.School)
                .AsQueryable();

            // 🔍 FILTER: SCHOOL NAME
            if (!string.IsNullOrEmpty(school))
            {
                query = query.Where(e => e.School.InstituteName.Contains(school));
            }

            // 📅 FILTER: FROM DATE
            if (!string.IsNullOrEmpty(fromDate))
            {
                var from = DateTime.Parse(fromDate);
                query = query.Where(e => e.EntryDate >= from);
            }

            // 📅 FILTER: TO DATE
            if (!string.IsNullOrEmpty(toDate))
            {
                var to = DateTime.Parse(toDate);
                query = query.Where(e => e.EntryDate <= to);
            }

            var enquiries = query
                .OrderByDescending(e => e.EntryDate)
                .Take(500) // safety limit
                .ToList();

            return View(enquiries);
        }
    }
}