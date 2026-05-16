using Microsoft.Extensions.Caching.Memory;
using SchoolProject.Data;
using SchoolProject.Models;

namespace SchoolProject.Services
{
    public class NavSyllabusItem
    {
        public string SyllabusName { get; set; } = "";
        public string SyllabusSlug { get; set; } = "";
        public string Url { get; set; } = "";
        public int SchoolCount { get; set; }
    }

    public class NavService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        public NavService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Cached for 1 hour — nav doesn't need to be live
        public List<NavSyllabusItem> GetSchoolNavItems(string citySlug = "bangalore")
        {
            var cacheKey = $"nav_syllabuses_{citySlug}";

            return _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                // Only show syllabuses that actually have schools
                // Uses junction table so open-schooling etc. are included
                var items = _context.Syllabuses
                    .Where(s => s.SyllabusSlug != null && s.SyllabusName != null)
                    .Select(s => new NavSyllabusItem
                    {
                        SyllabusName = s.SyllabusName!,
                        SyllabusSlug = s.SyllabusSlug!,
                        Url = $"/{s.SyllabusSlug}-schools-in-{citySlug}",
                        SchoolCount = s.Schools!
                            .SelectMany(sc => sc.SchoolSyllabuses!)
                            .Select(ss => ss.InstituteId)
                            .Distinct()
                            .Count()
                    })
                    .ToList()
                    // Only show syllabuses that have at least 1 school
                    .Where(x => x.SchoolCount > 0)
                    .OrderBy(x => x.SyllabusName)
                    .ToList();

                return items;
            })!;
        }
    }
}