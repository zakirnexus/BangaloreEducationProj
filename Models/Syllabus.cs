using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("tb_syllabus", Schema = "dbo")]
    public class Syllabus
    {
        [Key]
        [Column("syllabus_id")]
        public int SyllabusId { get; set; }

        [Column("syllabus")]
        public string? SyllabusName { get; set; }

        [Column("syllabus_slug")]
        public string? SyllabusSlug { get; set; }

        public ICollection<School>? Schools { get; set; }
    }
}