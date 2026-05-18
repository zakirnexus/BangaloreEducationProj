using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("response")]
    public class Enquiry
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("InstituteId")]
        public int? InstituteId { get; set; }

        [ForeignKey("InstituteId")]
        public School? School { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("Phone")]
        public string? Phone { get; set; }

        [Column("Enquiry")]
        public string? Message { get; set; }

        [Column("Institute")]
        public string? College { get; set; }

        [Column("Locality")]
        public string? Locality { get; set; }

        [Column("course")]
        public string? Course { get; set; }

        [Column("classfn")]
        public string? ClassFn { get; set; }

        [Column("PageUrl")]
        public string? PageUrl { get; set; }

        [Column("QueryType")]
        public string? QueryType { get; set; }

        [Column("EntryDate")]
        public DateTime? EntryDate { get; set; }
    }
}