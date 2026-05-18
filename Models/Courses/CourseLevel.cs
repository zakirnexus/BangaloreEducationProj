using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Courses
{
    [Table("tb_course_levels")]
    public class CourseLevel
    {
        [Key]
        [Column("level_id")]
        public int LevelId { get; set; }

        [Column("level_name")]
        public string? LevelName { get; set; }

        [Column("level_slug")]
        public string? LevelSlug { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
