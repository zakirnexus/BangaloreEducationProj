using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Lookups
{
    [Table("tb_inst_ownership")]
    public class InstOwnership

    {
        [Key]
        [Column("inst_ownership_id")]
        public int InstOwnershipId { get; set; }

        [Column("inst_ownership_type")]
        public string? InstOwnershipType { get; set; }
    }
}
