using SchoolProject.Data;
using System.Linq;

namespace SchoolProject.Services
{
    public class ContentService
    {
        private readonly AppDbContext _context;

        public ContentService(AppDbContext context)
        {
            _context = context;
        }

        public string? GetContent(string pageType, int? cityId, int? syllabusId, string section)
        {
            return _context.DynamicContents
                .Where(c =>
                    c.PageType == pageType &&
                    c.Section == section &&
                    c.IsActive == true &&  // ← Fixed: added == true
                    (c.CityId == cityId || c.CityId == null) &&
                    (c.SyllabusId == syllabusId || c.SyllabusId == null)
                )
                .OrderByDescending(c => c.CityId.HasValue)
                .ThenByDescending(c => c.SyllabusId.HasValue)
                .Select(c => c.Content)
                .FirstOrDefault();
        }
    }
}