using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("tb_admin_users")]
    public class AdminUser
    {
        [Column("admin_id")]
        [Key]
        public int AdminId { get; set; }

        [Column("username")]
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Column("email")]
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("full_name")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Column("role")]
        [StringLength(20)]
        public string Role { get; set; } = "Admin";  // Admin, Editor

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}