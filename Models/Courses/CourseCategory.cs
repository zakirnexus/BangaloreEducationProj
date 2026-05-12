using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_course_categories")]
    public class CourseCategory
    {
        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("category_name")]
        public string? CategoryName { get; set; }

        [Column("category_slug")]
        public string? CategorySlug { get; set; }

        [Column("parent_category_id")]
        public int? ParentCategoryId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public CourseCategory? ParentCategory { get; set; }
        public ICollection<CourseCategory>? SubCategories { get; set; }
    }
}
