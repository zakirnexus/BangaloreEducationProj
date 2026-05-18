using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Lookups
{
    [Table("tb_localities")]
    public class Locality
    {
        [Key]
        [Column("locality_id")]
        public int LocalityId { get; set; }

        [Column("city_id")]
        public int CityId { get; set; }

        [Column("locality_name")]
        public string? LocalityName { get; set; }

        [Column("locality_slug")]
        public string? LocalitySlug { get; set; }

        [Column("nsewc")]
        public string? Nsewc { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public City? City { get; set; }
        public ICollection<SchoolProject.Models.School>? Schools { get; set; }
        public ICollection<SchoolProject.Models.Colleges.College>? Colleges { get; set; }
    }
}
