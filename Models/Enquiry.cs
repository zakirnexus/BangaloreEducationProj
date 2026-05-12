using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("response")]
    public class Enquiry
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("institute_id")]
        public int? InstituteId { get; set; }

        [ForeignKey("InstituteId")]
        public School? School { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("Enquiry")]
        public string? Message { get; set; }

        [Column("Institute")]
        public string? College { get; set; }

        [Column("course")]
        public string? Course { get; set; }

        [Column("classfn")]
        public string? ClassFn { get; set; }

        [Column("page_url")]
        public string? PageUrl { get; set; }

        [Column("query_type")]
        public string? QueryType { get; set; }

        [Column("entry_date")]
        public DateTime? EntryDate { get; set; }
    }
}