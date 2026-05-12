using Microsoft.AspNetCore.Mvc;
using SchoolProject.Data;
using SchoolProject.Services;
using System.Linq;

namespace SchoolProject.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SidebarService _sidebarService;

        public BlogController(AppDbContext context, SidebarService sidebarService)
        {
            _context = context;
            _sidebarService = sidebarService;
        }

        // ✅ LIST PAGE (/blog)
        [HttpGet("blog")]
        public IActionResult Index()
        {
            var posts = _context.BlogPosts
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            ViewBag.Sidebar = _sidebarService.GetBlogListSidebar();

            return View(posts);
        }

        // ✅ DETAILS PAGE (/blog/title-slug)
        [HttpGet("blog/{slug}")]
        public IActionResult Details(string slug)
        {
            var post = _context.BlogPosts
                .FirstOrDefault(x => x.Slug == slug && x.IsActive);

            if (post == null)
                return NotFound();

            ViewBag.Sidebar = _sidebarService.GetBlogDetailSidebar(post);

            return View(post);
        }
    }
}
