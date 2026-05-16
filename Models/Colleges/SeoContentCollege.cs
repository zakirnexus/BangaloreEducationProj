using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Colleges
{
    [Table("tb_seo_content_colleges")]
    public class SeoContentCollege
    {
        [Key]
        [Column("seo_id")]
        public int SeoId { get; set; }

        [Column("city_id")]
        public int? CityId { get; set; }

        [Column("course_id")]
        public int? CourseId { get; set; }

        [Column("page_type")]
        public string? PageType { get; set; }

        [Column("section")]
        public string? Section { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
