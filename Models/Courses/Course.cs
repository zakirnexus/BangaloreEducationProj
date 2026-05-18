using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_courses")]
    public class Course
    {
        [Key]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("course_name")]
        public string? CourseName { get; set; }

        [Column("course_slug")]
        public string? CourseSlug { get; set; }

        [Column("course_full_name")]
        public string? CourseFullName { get; set; }

        [Column("level_id")]
        public int LevelId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("duration_years")]
        public int? DurationYears { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public CourseLevel? Level { get; set; }
        public CourseCategory? Category { get; set; }      

        [Column("short_name")]
        public string? ShortName { get; set; }
    }
}
