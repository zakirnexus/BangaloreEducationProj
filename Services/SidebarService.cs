using SchoolProject.Data;
using SchoolProject.Models;
using SchoolProject.Models.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SchoolProject.Services
{
    public class SidebarService
    {
        private readonly AppDbContext _context;

        public SidebarService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ SCHOOL DETAIL SIDEBAR
        public SidebarViewModel GetSchoolSidebar(School school)
        {
            var sidebar = new SidebarViewModel();

            var relatedSchools = _context.Schools
                .Include(s => s.City)
                .Where(s =>
                    s.CityId == school.CityId &&
                    s.SyllabusId == school.SyllabusId &&
                    s.InstituteId != school.InstituteId)
                .Take(8)
                .Select(s => new SidebarItem
                {
                    Title = s.InstituteName,
                    Url = "/school/" + s.InstituteSlug,
                    Subtitle = s.City.CityName
                })
                .ToList();

            sidebar.Sections.Add(new SidebarSection
            {
                Heading = $"{school.Syllabus?.SyllabusName} Schools in {school.City?.CityName}",
                Items = relatedSchools
            });

            return sidebar;
        }

        // ✅ SCHOOL LIST SIDEBAR (e.g. cbse-schools-in-bangalore)
        // citySlug is passed in directly from the URL — do NOT derive it from cityName
        public SidebarViewModel GetSchoolListSidebar(int cityId, int syllabusId, string cityName, string syllabusName, string citySlug)
        {
            var sidebar = new SidebarViewModel();

            // 🔹 OTHER BOARDS IN SAME CITY
            var otherBoardSlugs = _context.Schools
                .Include(s => s.Syllabus)
                .Where(s => s.CityId == cityId && s.SyllabusId != syllabusId && s.Syllabus != null)
                .Select(s => new { s.Syllabus!.SyllabusName, s.Syllabus.SyllabusSlug })
                .ToList()
                .DistinctBy(x => x.SyllabusSlug)
                .Take(6)
                .ToList();

            if (otherBoardSlugs.Any())
            {
                sidebar.Sections.Add(new SidebarSection
                {
                    Heading = $"Other Boards in {cityName}",
                    Items = otherBoardSlugs.Select(b => new SidebarItem
                    {
                        Title = $"{b.SyllabusName} Schools in {cityName}",
                        Url = $"/{b.SyllabusSlug}-schools-in-{citySlug}"  // ✅ use DB slug
                    }).ToList()
                });
            }

            // 🔹 SAME BOARD IN OTHER CITIES
            var otherCitySlugs = _context.Schools
                .Include(s => s.City)
                .Where(s => s.SyllabusId == syllabusId && s.CityId != cityId && s.City != null)
                .Select(s => new { s.City!.CityName, s.City.CitySlug })
                .ToList()
                .DistinctBy(x => x.CitySlug)
                .Take(6)
                .ToList();

            if (otherCitySlugs.Any())
            {
                var boardSlug = syllabusName.ToLower();
                sidebar.Sections.Add(new SidebarSection
                {
                    Heading = $"{syllabusName} Schools in Other Cities",
                    Items = otherCitySlugs.Select(c => new SidebarItem
                    {
                        Title = $"{syllabusName} Schools in {c.CityName}",
                        Url = $"/{boardSlug}-schools-in-{c.CitySlug}"  // ✅ already uses DB slug
                    }).ToList()
                });
            }

            // 🔹 FEATURED SCHOOLS — top 6 by ListingRank, non-sponsored
            var featuredSchools = _context.Schools
                .Include(s => s.SchoolSyllabuses!)
                .Where(s =>
                    s.CityId == cityId &&
                    s.SchoolSyllabuses!.Any(ss => ss.SyllabusId == syllabusId) &&
                    s.IsSponsored == false &&
                    s.ListingRank != null &&
                    s.ListingRank > 0)
                .OrderBy(s => s.ListingRank)
                .Take(6)
                .Select(s => new SidebarItem
                {
                    Title = s.InstituteName!,
                    Url   = "/school/" + s.InstituteSlug
                })
                .ToList();

            if (featuredSchools.Any())
            {
                sidebar.Sections.Add(new SidebarSection
                {
                    Heading = $"Top {syllabusName} Schools in {cityName}",
                    Items   = featuredSchools
                });
            }

            return sidebar;
        }

        // ✅ BLOG LIST SIDEBAR
        public SidebarViewModel GetBlogListSidebar()
        {
            var sidebar = new SidebarViewModel();

            var recentPosts = _context.BlogPosts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .Take(8)
                .Select(p => new SidebarItem
                {
                    Title = p.Title ?? "",
                    Url = "/blog/" + p.Slug,
                    Subtitle = p.CreatedDate.ToString("dd MMM yyyy")
                })
                .ToList();

            if (recentPosts.Any())
            {
                sidebar.Sections.Add(new SidebarSection
                {
                    Heading = "Recent Posts",
                    Items = recentPosts
                });
            }

            var cities = _context.BlogPosts
                .Where(p => p.IsActive && p.City != null)
                .Select(p => p.City!)
                .Distinct()
                .Take(8)
                .ToList();

            if (cities.Any())
            {
                sidebar.Sections.Add(new SidebarSection
                {
                    Heading = "Browse by City",
                    Items = cities.Select(c => new SidebarItem
                    {
                        Title = c,
                        Url = "/blog?city=" + Uri.EscapeDataString(c)
                    }).ToList()
                });
            }

            return sidebar;
        }

        // ✅ BLOG DETAIL SIDEBAR
        public SidebarViewModel GetBlogDetailSidebar(BlogPost post)
        {
            var sidebar = new SidebarViewModel();

            var related = _context.BlogPosts
                .Where(p =>
                    p.IsActive &&
                    p.Id != post.Id &&
                    (p.City == post.City || p.Syllabus == post.Syllabus))
                .OrderByDescending(p => p.CreatedDate)
                .Take(6)
                .Select(p => new SidebarItem
                {
                    Title = p.Title ?? "",
                    Url = "/blog/" + p.Slug,
                    Subtitle = p.CreatedDate.ToString("dd MMM yyyy")
                })
                .ToList();

            if (!related.Any())
            {
                related = _context.BlogPosts
                    .Where(p => p.IsActive && p.Id != post.Id)
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(6)
                    .Select(p => new SidebarItem
                    {
                        Title = p.Title ?? "",
                        Url = "/blog/" + p.Slug,
                        Subtitle = p.CreatedDate.ToString("dd MMM yyyy")
                    })
                    .ToList();
            }

            if (related.Any())
            {
                sidebar.Sections.Add(new SidebarSection
                {
                    Heading = "Related Posts",
                    Items = related
                });
            }

            if (!string.IsNullOrEmpty(post.City))
            {
                var citySchools = _context.Schools
                    .Include(s => s.City)
                    .Where(s => s.City != null && s.City.CityName == post.City)
                    .Take(5)
                    .Select(s => new SidebarItem
                    {
                        Title = s.InstituteName,
                        Url = "/school/" + s.InstituteSlug,
                        Subtitle = s.City!.CityName
                    })
                    .ToList();

                if (citySchools.Any())
                {
                    sidebar.Sections.Add(new SidebarSection
                    {
                        Heading = $"Schools in {post.City}",
                        Items = citySchools
                    });
                }
            }

            return sidebar;
        }
    }
}


