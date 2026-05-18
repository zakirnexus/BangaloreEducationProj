using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolProject.Data;
using SchoolProject.Models.Colleges;
using SchoolProject.Models.Courses;
using SchoolProject.Services;

namespace SchoolProject.Controllers
{
    public class CollegeEnquiryDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public int InstituteId { get; set; }
        public int? CourseId { get; set; }
        public string? College { get; set; }
        public string? PageUrl { get; set; }
        public string? QueryType { get; set; }
        public string? Honeypot { get; set; }
        public string? RecaptchaToken { get; set; }
    }

    public class CollegeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ContentService _contentService;
        private readonly EmailService _emailService;
        private readonly ReCaptchaService _recaptchaService;
        private readonly IMemoryCache _cache;

        public CollegeController(
            AppDbContext context,
            ContentService contentService,
            EmailService emailService,
            ReCaptchaService recaptchaService,
            IMemoryCache cache)
        {
            _context = context;
            _contentService = contentService;
            _emailService = emailService;
            _recaptchaService = recaptchaService;
            _cache = cache;
        }

        // ===== SEO URL: /{course}-colleges-in-{city} =====
        [HttpGet]
        [Route("{course}-colleges-in-{city}")]
        public IActionResult List(
            string course,
            string city,
            int page = 1,
            string? locality = null,
            string? nsewc = null,
            int? coedId = null,
            int? ownershipId = null,
            string? feesRange = null)
        {
            // Trailing slash redirect
            if (Request.Path.Value?.EndsWith("/") == true)
            {
                var canonical = Request.Path.Value.TrimEnd('/');
                if (Request.QueryString.HasValue)
                    canonical += Request.QueryString.Value;
                return RedirectPermanent(canonical);
            }

            int pageSize = 10;

            // Find course by slug
            var courseObj = _context.Courses
                .Include(c => c.Level)
                .Include(c => c.Category)
                .FirstOrDefault(c => c.CourseSlug != null &&
                                     c.CourseSlug.ToLower() == course.ToLower());

            if (courseObj == null)
            {
                // Try finding by category slug
                var category = _context.CourseCategories
                    .FirstOrDefault(c => c.CategorySlug != null &&
                                         c.CategorySlug.ToLower() == course.ToLower());

                if (category == null)
                    return Content($"No course or category found for: '{course}'");

                return ListByCategory(category.CategoryId, city, page, locality, nsewc, coedId, ownershipId, feesRange);
            }

            city = city.Trim('/');

            var cityObj = _context.Cities
                .FirstOrDefault(c => c.CitySlug != null &&
                                     c.CitySlug.ToLower() == city.ToLower());

            if (cityObj == null)
                return Content($"No city found for: '{city}'");

            // Base query
            var baseQuery = _context.Colleges
                .Include(c => c.City)
                .Include(c => c.Ownership)
                .Include(c => c.Coed)
                .Include(c => c.Locality)
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.Course)
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.SpecializationNav)
                .Where(c =>
                    c.CityId == cityObj.CityId &&
                    c.IsActive &&
                    c.CollegeCourses!.Any(cc => cc.CourseId == courseObj.CourseId && cc.IsActive));

            // Filter dropdowns
            ViewBag.Localities = _context.Localities
                .Where(l => l.CityId == cityObj.CityId &&
                            l.Colleges!.Any(c => c.CollegeCourses!.Any(cc => cc.CourseId == courseObj.CourseId)))
                .OrderBy(l => l.LocalityName)
                .Select(l => new { l.LocalityId, l.LocalityName })
                .ToList();

            ViewBag.NsewcOptions = _context.Localities
                .Where(l => l.CityId == cityObj.CityId &&
                            l.Nsewc != null && l.Nsewc != "" &&
                            l.Colleges!.Any(c => c.CollegeCourses!.Any(cc => cc.CourseId == courseObj.CourseId)))
                .Select(l => l.Nsewc!.ToLower())
                .Distinct()
                .ToList();

            ViewBag.CoedOptions = _context.Coeds
                .Where(co => baseQuery.Any(c => c.CoedId == co.CoedId))
                .OrderBy(co => co.CoedId)
                .Select(co => new { co.CoedId, co.CoedName })
                .ToList();

            ViewBag.OwnerOptions = baseQuery
                .Include(c => c.Ownership)
                .Where(c => c.Ownership != null && c.Ownership.InstOwnershipType != null)
                .Select(c => new { c.InstOwnershipId, Type = c.Ownership!.InstOwnershipType })
                .Distinct()
                .OrderBy(o => o.Type)
                .ToList();

            // Apply filters
            var query = baseQuery;

            if (!string.IsNullOrWhiteSpace(locality) && int.TryParse(locality, out int localityId))
                query = query.Where(c => c.LocalityId == localityId);

            if (!string.IsNullOrWhiteSpace(nsewc))
                query = query.Where(c => c.Locality != null &&
                                         c.Locality.Nsewc != null &&
                                         c.Locality.Nsewc.ToLower() == nsewc.ToLower());

            if (coedId.HasValue)
                query = query.Where(c => c.CoedId == coedId.Value);

            if (ownershipId.HasValue)
                query = query.Where(c => c.InstOwnershipId == ownershipId.Value);

            // Ordering
            var ordered = query
                .OrderBy(c =>
                    c.ListingRank == null || c.ListingRank == 0 ? 2 :
                    c.IsSponsored ? 1 : 0)
                .ThenBy(c => c.ListingRank == 0 || c.ListingRank == null ? c.InstituteName : null)
                .ThenBy(c => c.ListingRank);

            // Pagination
            int totalRecords = query.Count();
            var collegeList = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ViewBag
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Course = course;
            ViewBag.City = city;
            ViewBag.CourseName = courseObj.CourseName;
            ViewBag.CityName = cityObj.CityName;

            ViewBag.SelLocality = (!string.IsNullOrWhiteSpace(locality) && int.TryParse(locality, out int selLid)) ? selLid : (int?)null;
            ViewBag.SelNsewc = nsewc;
            ViewBag.SelCoed = coedId;
            ViewBag.SelOwnership = ownershipId;
            ViewBag.SelFees = feesRange;
            ViewBag.FiltersActive = locality != null || nsewc != null || coedId != null ||
                                    ownershipId != null || feesRange != null;

            ViewBag.ShowFilterPanel = true;

            ViewBag.Title = $"{courseObj.CourseName} Colleges in {cityObj.CityName} ({totalRecords} Colleges)";
            if (page > 1) ViewBag.Title += $" - Page {page}";

            ViewBag.Description = $"Explore {totalRecords} {courseObj.CourseName} colleges in {cityObj.CityName}. Compare fees, admission details, placements and more.";

            var seoContent = _context.SeoContentColleges.FirstOrDefault(x =>
                x.CityId == cityObj.CityId &&
                x.CourseId == courseObj.CourseId &&
                x.PageType == "List" &&
                x.Section == "Top" &&
                x.IsActive);

            ViewBag.TopContent = seoContent?.Content;

            return View("Index", collegeList);
        }

        // List by category
        private IActionResult ListByCategory(int categoryId, string city, int page,
            string? locality, string? nsewc, int? coedId, int? ownershipId, string? feesRange)
        {
            int pageSize = 10;

            var cityObj = _context.Cities
                .FirstOrDefault(c => c.CitySlug != null && c.CitySlug.ToLower() == city.ToLower());

            if (cityObj == null)
                return Content($"No city found for: '{city}'");

            var category = _context.CourseCategories.Find(categoryId);

            // Base query with category filter
            var baseQuery = _context.Colleges
                .Include(c => c.City)
                .Include(c => c.Ownership)
                .Include(c => c.Coed)
                .Include(c => c.Locality)
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.Course)
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.SpecializationNav)
                .Where(c =>
                    c.CityId == cityObj.CityId &&
                    c.IsActive &&
                    c.CollegeCourses!.Any(cc => cc.Course!.CategoryId == categoryId && cc.IsActive));

            // Apply filters
            var query = baseQuery;

            if (!string.IsNullOrWhiteSpace(locality) && int.TryParse(locality, out int localityId))
                query = query.Where(c => c.LocalityId == localityId);

            if (!string.IsNullOrWhiteSpace(nsewc))
                query = query.Where(c => c.Locality != null &&
                                         c.Locality.Nsewc != null &&
                                         c.Locality.Nsewc.ToLower() == nsewc.ToLower());

            if (coedId.HasValue)
                query = query.Where(c => c.CoedId == coedId.Value);

            if (ownershipId.HasValue)
                query = query.Where(c => c.InstOwnershipId == ownershipId.Value);

            // Ordering
            var ordered = query
                .OrderBy(c =>
                    c.ListingRank == null || c.ListingRank == 0 ? 2 :
                    c.IsSponsored ? 1 : 0)
                .ThenBy(c => c.ListingRank == 0 || c.ListingRank == null ? c.InstituteName : null)
                .ThenBy(c => c.ListingRank);

            // Pagination
            int totalRecords = query.Count();
            var collegeList = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ViewBag - ALL properties the view expects
            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.Course = category?.CategorySlug ?? "category";
            ViewBag.City = city;
            ViewBag.CourseName = category?.CategoryName;
            ViewBag.CityName = cityObj.CityName;

            ViewBag.SelLocality = (!string.IsNullOrWhiteSpace(locality) && int.TryParse(locality, out int selLid)) ? selLid : (int?)null;
            ViewBag.SelNsewc = nsewc;
            ViewBag.SelCoed = coedId;
            ViewBag.SelOwnership = ownershipId;
            ViewBag.SelFees = feesRange;
            ViewBag.FiltersActive = locality != null || nsewc != null || coedId != null ||
                                    ownershipId != null || feesRange != null;

            ViewBag.ShowFilterPanel = true;

            // Filter dropdowns for category pages
            ViewBag.Localities = _context.Localities
                .Where(l => l.CityId == cityObj.CityId &&
                            l.Colleges!.Any(c => c.CollegeCourses!.Any(cc => cc.Course!.CategoryId == categoryId)))
                .OrderBy(l => l.LocalityName)
                .Select(l => new { l.LocalityId, l.LocalityName })
                .ToList();

            ViewBag.NsewcOptions = _context.Localities
                .Where(l => l.CityId == cityObj.CityId &&
                            l.Nsewc != null && l.Nsewc != "" &&
                            l.Colleges!.Any(c => c.CollegeCourses!.Any(cc => cc.Course!.CategoryId == categoryId)))
                .Select(l => l.Nsewc!.ToLower())
                .Distinct()
                .ToList();

            ViewBag.CoedOptions = _context.Coeds
                .Where(co => baseQuery.Any(c => c.CoedId == co.CoedId))
                .OrderBy(co => co.CoedId)
                .Select(co => new { co.CoedId, co.CoedName })
                .ToList();

            ViewBag.OwnerOptions = baseQuery
                .Include(c => c.Ownership)
                .Where(c => c.Ownership != null && c.Ownership.InstOwnershipType != null)
                .Select(c => new { c.InstOwnershipId, Type = c.Ownership!.InstOwnershipType })
                .Distinct()
                .OrderBy(o => o.Type)
                .ToList();

            ViewBag.Title = $"{category?.CategoryName} Colleges in {cityObj.CityName} ({totalRecords} Colleges)";
            if (page > 1) ViewBag.Title += $" - Page {page}";

            ViewBag.Description = $"Explore {totalRecords} {category?.CategoryName} colleges in {cityObj.CityName}. Compare fees, admission details, placements and more.";

            var seoContent = _context.SeoContentColleges.FirstOrDefault(x =>
                x.CityId == cityObj.CityId &&
                x.CourseId == null &&
                x.PageType == "List" &&
                x.Section == "Top" &&
                x.IsActive);

            ViewBag.TopContent = seoContent?.Content;

            return View("Index", collegeList);
        }

        // ===== DETAIL PAGE =====
        [HttpGet("college/{slug}")]
        public IActionResult Details(string slug)
        {
            var college = _context.Colleges
                .Include(c => c.City)
                .Include(c => c.Locality)
                .Include(c => c.Coed)
                .Include(c => c.Ownership)
                .Include(c => c.State)
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.Course)
                .ThenInclude(course => course!.Level)
                .Include(c => c.CollegeCourses!)
                .ThenInclude(cc => cc.SpecializationNav)
                .FirstOrDefault(c => c.InstituteSlug == slug && c.IsActive);

            if (college == null)
            {
                // Try case-insensitive
                college = _context.Colleges
                    .Include(c => c.City)
                    .Include(c => c.Locality)
                    .Include(c => c.Coed)
                    .Include(c => c.Ownership)
                    .Include(c => c.State)
                    .Include(c => c.CollegeCourses!)
                    .ThenInclude(cc => cc.Course)
                    .ThenInclude(course => course!.Level)
                    .Include(c => c.CollegeCourses!)
                    .ThenInclude(cc => cc.SpecializationNav)
                    .FirstOrDefault(c => 
                        c.InstituteSlug != null && 
                        c.InstituteSlug.ToLower() == slug.ToLower());
            }

            if (college == null)
                return Content($"College not found: '{slug}'");

            ViewBag.Title = $"{college.InstituteName} in {college.City?.CityName}";
            ViewBag.Description = $"{college.InstituteName} - Top college in {college.City?.CityName}. Check courses, fees, placements, admission.";
            ViewBag.Canonical = $"/college/{college.InstituteSlug}";
            ViewBag.InstituteId   = college.InstituteId;
            ViewBag.InstituteName = college.InstituteName;
            ViewBag.Photos        = college.Photos;

            ViewBag.RelatedColleges = _context.Colleges
                .Include(c => c.City)
                .Include(c => c.Locality)
                .Where(c => c.CityId == college.CityId && c.InstituteId != college.InstituteId && c.IsActive)
                .Take(6)
                .ToList();

            return View("Details", college);
        }

        // ===== ENQUIRY SUBMIT =====
        [HttpPost]
        [Route("College/Enquiry/Submit")]
        public async Task<IActionResult> SubmitEnquiry([FromBody] CollegeEnquiryDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "No data received" });

            if (!string.IsNullOrWhiteSpace(dto.Honeypot))
                return Ok(new { success = true });

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var rateLimitKey = $"college_enquiry_rate_{ip}";
            var minuteKey = $"college_enquiry_hour_{ip}";

            int minuteCount = _cache.GetOrCreate(rateLimitKey, e => {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });

            int hourCount = _cache.GetOrCreate(minuteKey, e => {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            if (minuteCount >= 3 || hourCount >= 10)
                return StatusCode(429, new { success = false, message = "Too many submissions. Please try again later." });

            _cache.Set(rateLimitKey, minuteCount + 1, TimeSpan.FromMinutes(1));
            _cache.Set(minuteKey, hourCount + 1, TimeSpan.FromHours(1));

            if (!string.IsNullOrWhiteSpace(dto.RecaptchaToken))
            {
                bool isHuman = await _recaptchaService.IsValidAsync(dto.RecaptchaToken, ip);
                if (!isHuman)
                    return BadRequest(new { success = false, message = "Verification failed." });
            }

            var dupKey = $"college_dup_{dto.Email}_{dto.InstituteId}";
            if (_cache.TryGetValue(dupKey, out _))
                return Ok(new { success = true });

            _cache.Set(dupKey, true, TimeSpan.FromMinutes(5));

            var enquiry = new CollegeEnquiry
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Message = dto.Message,
                InstituteId = dto.InstituteId,
                CourseId = dto.CourseId,
                PageUrl = dto.PageUrl,
                QueryType = dto.QueryType ?? "Admission Enquiry",
                EntryDate = DateTime.Now
            };

            try
            {
                _context.CollegeEnquiries.Add(enquiry);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("College enquiry save failed: " + ex.Message);
            }

            string? collegeEmail = null;
            if (dto.InstituteId > 0)
            {
                var college = _context.Colleges.Find(dto.InstituteId);
                collegeEmail = college?.Email;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(collegeEmail))
                {
                    _emailService.SendEnquiryEmail(
                        toEmail: collegeEmail,
                        instituteName: dto.College ?? "",
                        fromName: dto.Name ?? "",
                        fromEmail: dto.Email ?? "",
                        fromPhone: dto.Phone ?? "",
                        course: dto.CourseId?.ToString() ?? "",
                        message: dto.Message ?? "",
                        queryType: dto.QueryType ?? "Admission Enquiry",
                        pageUrl: dto.PageUrl ?? ""
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("College email send failed: " + ex.Message);
            }

            return Ok(new { success = true });
        }
    }
}
