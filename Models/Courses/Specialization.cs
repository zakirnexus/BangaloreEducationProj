using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolProject.Models.Colleges;

namespace SchoolProject.Models.Courses
{
    [Table("tb_specializations")]
    public class Specialization
    {
        [Column("specialization_id")]
        [Key]
        public int SpecializationId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("specialization_name")]
        [Required]
        [StringLength(150)]
        public string SpecializationName { get; set; } = string.Empty;

        [Column("specialization_slug")]
        [StringLength(150)]
        public string? SpecializationSlug { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Navigation
        public Course? Course { get; set; }
        public ICollection<CollegeCourse>? CollegeCourses { get; set; }
    }
}