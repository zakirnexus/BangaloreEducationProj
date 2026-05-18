using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolProject.Models.Courses;

namespace SchoolProject.Models.Colleges
{
    [Table("tb_college_enquiries")]
    public class CollegeEnquiry
    {
        [Key]
        [Column("enquiry_id")]
        public int EnquiryId { get; set; }

        [Column("institute_id")]
        [ForeignKey("College")]
        public int? InstituteId { get; set; }

        [Column("course_id")]
        [ForeignKey("Course")]
        public int? CourseId { get; set; }

        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }

        [Column("page_url")]
        public string? PageUrl { get; set; }

        [Column("query_type")]
        public string? QueryType { get; set; }

        [Column("entry_date")]
        public DateTime? EntryDate { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; }

        [Column("is_responded")]
        public bool IsResponded { get; set; }

        public College? College { get; set; }
        public Course? Course { get; set; }
    }
}