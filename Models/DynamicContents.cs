using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("DynamicContents")]
    public class DynamicContents
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PageType")]
        public string? PageType { get; set; }

        [Column("CityId")]
        public int? CityId { get; set; }

        [Column("SyllabusId")]
        public int? SyllabusId { get; set; }

        [Column("Section")]
        public string? Section { get; set; }

        [Column("Title")]
        public string? Title { get; set; }

        [Column("Content")]
        public string? Content { get; set; }

        [Column("IsActive")]
        public bool? IsActive { get; set; }
    }
}