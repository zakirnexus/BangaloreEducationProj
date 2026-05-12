using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models.Lookups
{
    [Table("tb_states")]  // ← ADD THIS
    public class State
    {
        [Key]
        [Column("state_id")]
        public int StateId { get; set; }

        [Column("state_name")]
        public string? StateName { get; set; }

        [Column("state_slug")]
        public string? StateSlug { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
