using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("tb_seo_content")]
    public class SeoContent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("city_id")]
        public int? CityId { get; set; }

        [Column("syllabus_id")]
        public int? SyllabusId { get; set; }

        [Column("content")]
        public string? Content { get; set; }

        [Column("page_type")]
        public string? PageType { get; set; }

        [Column("section")]
        public string? Section { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; }
    }
}