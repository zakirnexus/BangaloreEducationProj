using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SchoolProject.Data;
using SchoolProject.Models;
using SchoolProject.Models.ViewModels;
using SchoolProject.Services;

namespace SchoolProject.Controllers
{
    public class EnquiryDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Course { get; set; }
        public string? Message { get; set; }
        public int InstituteId { get; set; }
        public string? College { get; set; }
        public string? PageUrl { get; set; }
        public string? QueryType { get; set; }
        public string? Honeypot { get; set; }
        public string? RecaptchaToken { get; set; }
    }

    public class SchoolController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SidebarService _sidebarService;
        private readonly ContentService _contentService;
        private readonly EmailService _emailService;
        private readonly ReCaptchaService _recaptchaService;
        private readonly IMemoryCache _cache;

        public SchoolController(
            AppDbContext context,
            SidebarService sidebarService,
            ContentService contentService,
            EmailService emailService,
            ReCaptchaService recaptchaService,
            IMemoryCache cache)
        {
            _context = context;
            _sidebarService = sidebarService;
            _contentService = contentService;
            _emailService = emailService;
            _recaptchaService = recaptchaService;
            _cache = cache;
        }

        private static int? ParseFeeMin(string? feesStructure)
        {
            if (string.IsNullOrWhiteSpace(feesStructure)) return null;
            var digits = new string(feesStructure.Where(char.IsDigit).ToArray());
            return digits.Length > 0 && int.TryParse(digits.Substring(0, Math.Min(digits.Length, 9)), out var val)
                ? val : (int?)null;
        }

        [HttpGet]
        [Route("{board}-schools-in-{city}")]
        public IActionResult List(
            string board,
            string city,
            int page = 1,
            string? locality = null,
            string? nsewc = null,
            int? coedId = null,
            int? ownershipId = null,
            string? feesRange = null)
        {
            if (Request.Path.Value?.EndsWith("/") == true)
            {
                var canonical = Request.Path.Value.TrimEnd('/');
                if (Request.QueryString.HasValue)
                    canonical += Request.QueryString.Value;
                return RedirectPermanent(canonical);
            }

            int pageSize = 10;

            var syllabus = _context.Syllabuses
                .FirstOrDefault(s => s.SyllabusSlug != null &&
                                     s.SyllabusSlug.ToLower() == board.ToLower());

            if (syllabus == null) return Content("No syllabus found");

            city = city.Trim('/');

            var cityObj = _context.Cities
                .FirstOrDefault(c => c.CitySlug != null &&
                                     c.CitySlug.ToLower() == city.ToLower());

            if (cityObj == null)
            {
                var allSlugs = _context.Cities
                    .Where(c => c.CitySlug != null)
                    .Select(c => new { c.CityId, c.CityName, c.CitySlug })
                    .Take(30)
                    .ToList();
                return Content("No city found for: '" + city + "'\n\nAvailable slugs:\n" +
                    string.Join("\n", allSlugs.Select(x => $"id={x.CityId} | {x.CityName} | CitySlug={x.CitySlug}")));
            }

            var baseQuery = _context.Schools
                .Include(s => s.City)
                .Include(s => s.Ownership)
                .Include(s => s.Coed)
                .Include(s => s.Locality)
                .Include(s => s.SchoolSyllabuses!)
                .Where(s =>
                    s.CityId == cityObj.CityId &&
                    s.SchoolSyllabuses!.Any(ss => ss.SyllabusId == syllabus.SyllabusId));

            ViewBag.Localities = _context.Localities
                .Where(l => l.CityId == cityObj.CityId &&
                            l.Schools!.Any(s => s.SchoolSyllabuses!.Any(ss => ss.SyllabusId == syllabus.SyllabusId)))
                .OrderBy(l => l.LocalityName)
                .Select(l => new { l.LocalityId, l.LocalityName })
                .ToList<dynamic>();

            ViewBag.NsewcOptions = _context.Localities
                .Where(l => l.CityId == cityObj.CityId &&
                            l.Nsewc != null && l.Nsewc != "" &&
                            l.Schools!.Any(s => s.SchoolSyllabuses!.Any(ss => ss.SyllabusId == syllabus.SyllabusId)))
                .Select(l => l.Nsewc!.ToLower())
                .Distinct()
                .ToList();

            ViewBag.CoedOptions = _context.Coeds
                .Where(c => baseQuery.Any(s => s.CoedId == c.CoedId))
                .OrderBy(c => c.CoedId)
                .Select(c => new { c.CoedId, c.CoedName })
                .ToList<dynamic>();

            ViewBag.OwnerOptions = baseQuery
                .Include(s => s.Ownership)
                .Where(s => s.Ownership != null && s.Ownership.InstOwnershipType != null)
                .Select(s => new { s.OwnershipId, Type = s.Ownership!.InstOwnershipType })
                .Distinct()
                .OrderBy(o => o.Type)
                .ToList<dynamic>();

            var query = baseQuery;

            if (!string.IsNullOrWhiteSpace(locality) && int.TryParse(locality, out int localityId))
                query = query.Where(s => s.LocalityId == localityId);

            if (!string.IsNullOrWhiteSpace(nsewc))
                query = query.Where(s => s.Locality != null &&
                                         s.Locality.Nsewc != null &&
                                         s.Locality.Nsewc.ToLower() == nsewc.ToLower());

            if (coedId.HasValue)
                query = query.Where(s => s.CoedId == coedId.Value);

            if (ownershipId.HasValue)
                query = query.Where(s => s.OwnershipId == ownershipId.Value);

            var ordered = query
                .OrderBy(s =>
                    s.ListingRank == null || s.ListingRank == 0 ? 2 :
                    s.IsSponsored ? 1 : 0)
                .ThenBy(s => s.ListingRank == 0 || s.ListingRank == null ? s.InstituteName : null)
                .ThenBy(s => s.ListingRank);

            int totalRecords;
            List<School> schoolList;

            if (!string.IsNullOrWhiteSpace(feesRange))
            {
                var parts = feesRange.Split('-');
                int feeMin = parts.Length >= 1 && int.TryParse(parts[0], out var lo) ? lo : 0;
                int feeMax = parts.Length >= 2 && int.TryParse(parts[1], out var hi) ? hi : int.MaxValue;

                var allMatched = ordered
                    .ToList()
                    .Where(s =>
                    {
                        var fee = ParseFeeMin(s.FeesStructure);
                        return fee.HasValue && fee.Value >= feeMin && fee.Value <= feeMax;
                    })
                    .ToList();

                totalRecords = allMatched.Count;
                schoolList   = allMatched.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                totalRecords = query.Count();
                schoolList   = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages   = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage  = page;
            ViewBag.Board        = board;
            ViewBag.City         = city;

            ViewBag.SelLocality  = (!string.IsNullOrWhiteSpace(locality) && int.TryParse(locality, out int selLid)) ? selLid : (int?)null;
            ViewBag.SelNsewc     = nsewc;
            ViewBag.SelCoed      = coedId;
            ViewBag.SelOwnership = ownershipId;
            ViewBag.SelFees      = feesRange;
            ViewBag.FiltersActive = locality != null || nsewc != null || coedId != null ||
                                    ownershipId != null || feesRange != null;

            ViewBag.ShowFilterPanel = true;

            ViewBag.Title = $"{syllabus.SyllabusSlug!.ToUpper()} Schools in {cityObj.CityName} ({totalRecords} Schools)";
            if (page > 1) ViewBag.Title += $" - Page {page}";

            ViewBag.Description = $"Explore {totalRecords} {syllabus.SyllabusSlug!.ToUpper()} schools in {cityObj.CityName}. Compare fees, admission details, reviews and more.";
            if (page > 1) ViewBag.Description += $" Page {page} of results.";

            var seoContent = _context.SeoContents.FirstOrDefault(x =>
            x.CityId == cityObj.CityId &&
            x.SyllabusId == syllabus.SyllabusId &&
            x.PageType == "List" &&
            x.Section == "Top" &&
            x.IsActive == true);
            ViewBag.TopContent    = seoContent?.Content;
            ViewBag.BottomContent = _contentService.GetContent("List", cityObj.CityId, syllabus.SyllabusId, "Bottom");
            ViewBag.Sidebar       = _sidebarService.GetSchoolListSidebar(
                cityObj.CityId,
                syllabus.SyllabusId,
                cityObj.CityName ?? city,
                syllabus.SyllabusName ?? board,
                cityObj.CitySlug ?? city);

            return View("Index", schoolList);
        }

        [HttpGet]
        [Route("schools")]
        public IActionResult AllSchools(int page = 1)
        {
            int pageSize = 20;

            var query = _context.Schools
                .Include(s => s.City)
                .Include(s => s.Syllabus);

            int totalRecords = query.Count();

            var schools = query
                .OrderBy(s =>
                    s.ListingRank == null || s.ListingRank == 0 ? 2 :
                    s.IsSponsored ? 1 : 0)
                .ThenBy(s => s.ListingRank == 0 || s.ListingRank == null ? s.InstituteName : null)
                .ThenBy(s => s.ListingRank)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalRecords = totalRecords;
            ViewBag.TotalPages   = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.CurrentPage  = page;
            ViewBag.Title        = $"All Schools in Bangalore ({totalRecords} Schools)";
            if (page > 1) ViewBag.Title += $" - Page {page}";
            ViewBag.Description  = $"Browse all {totalRecords} schools in Bangalore. Filter by syllabus, area, fees and more.";

            var syllabusLinks = _context.Syllabuses
                .Where(s => s.SyllabusSlug != null)
                .ToList()
                .Select(s => new SidebarItem
                {
                    Title = $"{s.SyllabusName} Schools in Bangalore",
                    Url   = $"/{s.SyllabusSlug}-schools-in-bangalore"
                })
                .ToList();

            var sidebar = new SchoolProject.Models.ViewModels.SidebarViewModel();
            sidebar.Sections.Add(new SchoolProject.Models.ViewModels.SidebarSection
            {
                Heading = "Browse by Syllabus",
                Items   = syllabusLinks
            });
            ViewBag.Sidebar = sidebar;

            return View("Index", schools);
        }

        [HttpGet("school/{slug}")]
        public IActionResult Details(string slug)
        {
            var school = _context.Schools
                .Include(s => s.City)
                .Include(s => s.Syllabus)
                .Include(s => s.SchoolSyllabuses!)
                    .ThenInclude(ss => ss.Syllabus)
                .FirstOrDefault(s => s.InstituteSlug == slug);

            if (school == null) return Content("Slug not found: " + slug);

            ViewBag.Sidebar       = _sidebarService.GetSchoolSidebar(school);
            ViewBag.DetailContent = _contentService.GetContent("Details", school.CityId, school.SyllabusId, "Bottom");
            ViewBag.Title         = $"{school.InstituteName} in {school.City?.CityName}";
            ViewBag.Description   = $"{school.InstituteName} is a {school.Syllabus?.SyllabusName} school located in {school.City?.CityName}.";
            ViewBag.Canonical     = $"/school/{school.InstituteSlug}";

            return View("Details", school);
        }

        [HttpPost]
        [Route("Enquiry/Submit")]
        public async Task<IActionResult> Submit([FromBody] EnquiryDto dto)
        {
            if (dto == null)
                return BadRequest(new { success = false, message = "No data received" });

            if (!string.IsNullOrWhiteSpace(dto.Honeypot))
            {
                Console.WriteLine("Honeypot triggered");
                return Ok(new { success = true });
            }

            var ip           = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var rateLimitKey = $"enquiry_rate_{ip}";
            var minuteKey    = $"enquiry_hour_{ip}";

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
                {
                    Console.WriteLine("reCAPTCHA failed");
                    return BadRequest(new { success = false, message = "Verification failed. Please try again." });
                }
            }

            var dupKey = $"dup_{dto.Email}_{dto.InstituteId}";
            if (_cache.TryGetValue(dupKey, out _))
                return Ok(new { success = true });

            _cache.Set(dupKey, true, TimeSpan.FromMinutes(5));

            var enquiry = new Enquiry
            {
                Name        = dto.Name,
                Email       = dto.Email,
                Phone       = dto.Phone,
                Course      = dto.Course,
                Message     = dto.Message,
                College     = dto.College,
                InstituteId = dto.InstituteId,
                PageUrl     = dto.PageUrl,
                QueryType   = dto.QueryType,
                EntryDate   = DateTime.Now,
                ClassFn     = "School Enquiry"
            };

            try
            {
                _context.Enquiries.Add(enquiry);
                _context.SaveChanges();
            }
            catch (Exception saveEx)
            {
                Console.WriteLine("Enquiry save failed: " + saveEx.Message);
            }

            string? phone       = null;
            string? schoolEmail = null;

            if (dto.InstituteId > 0)
            {
                var school = _context.Schools
                    .FirstOrDefault(s => s.InstituteId == dto.InstituteId);

                phone       = school?.Telephone;  // Changed from Phone
                schoolEmail = school?.Email;

                Console.WriteLine($"InstituteId: {dto.InstituteId}");
                Console.WriteLine($"School found: {school != null}");
                Console.WriteLine($"School email: '{schoolEmail}'");
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(schoolEmail))
                {
                    _emailService.SendEnquiryEmail(
                        toEmail:       schoolEmail,
                        instituteName: dto.College   ?? "",
                        fromName:      dto.Name      ?? "",
                        fromEmail:     dto.Email     ?? "",
                        fromPhone:     dto.Phone     ?? "",
                        course:        dto.Course    ?? "",
                        message:       dto.Message   ?? "",
                        queryType:     dto.QueryType ?? "Enquiry",
                        pageUrl:       dto.PageUrl   ?? ""
                    );
                }
            }
            catch (Exception emailEx)
            {
                Console.WriteLine("Email send failed: " + emailEx.Message);
                Console.WriteLine("Email stack: " + emailEx.StackTrace);
            }

            return Ok(new { success = true, phone = phone });
        }
    }
}