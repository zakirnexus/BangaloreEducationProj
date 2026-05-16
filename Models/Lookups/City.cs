using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Lookups
{
    [Table("tb_cities")]
    public class City
    {
        [Key]
        [Column("city_id")]
        public int CityId { get; set; }

        [Column("state_id")]
        public int? StateId { get; set; }

        [Column("city")]
        public string? CityName { get; set; }

        [Column("city_slug")]
        public string? CitySlug { get; set; }

        [Column("top_city")]
        public string? TopCity { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        public State? State { get; set; }
        public ICollection<Locality>? Localities { get; set; }
    }
}
