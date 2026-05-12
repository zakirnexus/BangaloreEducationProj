using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("BlogPosts")]
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Slug { get; set; }   // 👉 for internal pages

        public string? Content { get; set; } 

        public string? Image { get; set; } // stores file path

        public string? Url { get; set; } // optional (external blog link)

        public string? MetaTitle { get; set; }
        public string? MetaDescription { get; set; }

        public string? City { get; set; }       // e.g. Bangalore
        public string? Syllabus { get; set; }   // e.g. CBSE

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}