using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Lookups
{
    [Table("tb_coed")]
    public class Coed

    {
        [Key]
        [Column("coed_id")]
        public int CoedId { get; set; }

        [Column("coed")]
        public string? CoedName { get; set; }

        public ICollection<SchoolProject.Models.School>? Schools { get; set; }
    }
}
