using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Data;
using SchoolProject.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SchoolProject.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class AdminBlogController : Controller
    {
        private readonly AppDbContext _context;

        public AdminBlogController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ LIST
        public IActionResult Index()
        {
            var posts = _context.BlogPosts
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            return View(posts);
        }

        // ✅ CREATE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // ✅ CREATE (POST)
        [HttpPost]
        public IActionResult Create(BlogPost model, IFormFile? imageFile)
{
        if (!ModelState.IsValid)
        return View(model);

        // ✅ AUTO SLUG
        model.Slug = GenerateSlug(model.Title);

        // ✅ IMAGE UPLOAD
        		if (imageFile != null && imageFile.Length > 0)
		{
			var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

			if (!Directory.Exists(uploadDir))
			{
				Directory.CreateDirectory(uploadDir);
			}

			var fileName = Guid.NewGuid() + ".jpg"; // force jpg for compression
			var filePath = Path.Combine(uploadDir, fileName);

			using (var image = Image.Load(imageFile.OpenReadStream()))
			{
				image.Mutate(x => x.Resize(new ResizeOptions
				{
					Mode = ResizeMode.Max,
					Size = new Size(1200, 0) // max width 1200px
				}));

				image.Save(filePath, new JpegEncoder
				{
					Quality = 75 // compression
				});
			}

			model.Image = "/uploads/" + fileName;
		}

        _context.BlogPosts.Add(model);
        _context.SaveChanges();
        return RedirectToAction("Index");
        }

        // ✅ EDIT (GET)
        public IActionResult Edit(int id)
        {
            var post = _context.BlogPosts.Find(id);
            if (post == null) return NotFound();

            return View(post);
        }

        // ✅ EDIT (POST)
        [HttpPost]
	public IActionResult Edit(BlogPost model, IFormFile? imageFile)
	{
		var existing = _context.BlogPosts.Find(model.Id);
		if (existing == null) return NotFound();

		existing.Title = model.Title;
		existing.Content = model.Content;
		existing.City = model.City;
		existing.Syllabus = model.Syllabus;
		existing.IsActive = model.IsActive;

		existing.MetaTitle = model.MetaTitle;
		existing.MetaDescription = model.MetaDescription;

		// ✅ regenerate slug if title changed
		existing.Slug = GenerateSlug(model.Title);

		// ✅ image upload
		if (imageFile != null && imageFile.Length > 0)
		{
			var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
			var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

			using (var stream = new FileStream(path, FileMode.Create))
			{
				imageFile.CopyTo(stream);
			}

			existing.Image = "/uploads/" + fileName;
		}

		_context.SaveChanges();

		return RedirectToAction("Index");
	    }

        // ✅ DELETE
        public IActionResult Delete(int id)
        {
            var post = _context.BlogPosts.Find(id);
            if (post != null)
            {
                _context.BlogPosts.Remove(post);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        

    

        // ✅ SLUG GENERATOR
        private string GenerateSlug(string? title)
        {
        if (string.IsNullOrEmpty(title)) return "";

        return title
        .ToLower()
        .Replace(" ", "-")
        .Replace(",", "")
        .Replace(".", "")
        .Replace("/", "")
        .Replace("--", "-");
        }
        
    }
}