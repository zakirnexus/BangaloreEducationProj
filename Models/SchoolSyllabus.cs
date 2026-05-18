using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("school_syllabuses", Schema = "dbo")]
    public class SchoolSyllabus
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("institute_id")]
        public int InstituteId { get; set; }

        [Column("syllabus_id")]
        public int SyllabusId { get; set; }

        // Navigation properties
        [ForeignKey("InstituteId")]
        public School? School { get; set; }

        [ForeignKey("SyllabusId")]
        public Syllabus? Syllabus { get; set; }
    }
}