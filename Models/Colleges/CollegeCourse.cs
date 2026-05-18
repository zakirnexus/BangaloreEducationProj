using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolProject.Models.Courses;

namespace SchoolProject.Models.Colleges
{
    [Table("tb_college_courses")]
    public class CollegeCourse
    {
        [Column("college_course_id")]
        [Key]
        public int CollegeCourseId { get; set; }

        [Column("institute_id")]
        public int InstituteId { get; set; }

        [Column("course_id")]
        public int CourseId { get; set; }

        [Column("specialization_id")]
        public int? SpecializationId { get; set; }

        [Column("fees_structure")]
        public string? FeesStructure { get; set; }

        [Column("hostel_fees")]
        public string? HostelFees { get; set; }

        [Column("total_seats")]
        public int? TotalSeats { get; set; }

        [Column("available_seats")]
        public int? AvailableSeats { get; set; }

        [Column("entrance_exam")]
        public string? EntranceExam { get; set; }

        [Column("min_qualification")]
        public string? MinQualification { get; set; }

        [Column("course_description")]
        public string? CourseDescription { get; set; }

        [Column("course_duration")]
        public string? CourseDuration { get; set; }

        [Column("campus_placements")]
        public string? CampusPlacements { get; set; }

        [Column("additional_features")]
        public string? AdditionalFeatures { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("modified_date")]
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation
        public College? College { get; set; }
        public Course? Course { get; set; }
        public Specialization? SpecializationNav { get; set; }
    }
}