using SchoolProject.Models.Lookups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolProject.Models
{
    [Table("schools")]
    public class School
    {
        [Key]
        [Column("institute_id")]
        public int InstituteId { get; set; }

        [Column("institute_name")]
        public string? InstituteName { get; set; }

        [Column("institute_slug")]
        public string? InstituteSlug { get; set; }

        [Column("logo")]
        public string? Logo { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("inst_area")]
        public string? InstArea { get; set; }

        [Column("locality_id")]
        public int? LocalityId { get; set; }

        public Locality? Locality { get; set; }

        [NotMapped]
        public string? LocalityName => Locality?.LocalityName;

        [NotMapped]
        public string? Nsewc => Locality?.Nsewc;

        [Column("pincode")]
        public string? Pincode { get; set; }

        [Column("telephone")]
        public string? Telephone { get; set; }  // Changed from Phone

        [Column("email")]
        public string? Email { get; set; }

        [Column("email2")]
        public string? Email2 { get; set; }

        [Column("estd")]
        public string? Estd { get; set; }

        [Column("accreditation")]
        public string? Accreditation { get; set; }

        [Column("approved_by")]
        public string? ApprovedBy { get; set; }  // Changed from Approved_By

        [Column("status")]
        public string? Status { get; set; }

        [Column("coed_id")]
        public int? CoedId { get; set; }

        [ForeignKey("CoedId")]
        public Coed? Coed { get; set; }

        [NotMapped]
        public string? CoedName => Coed?.CoedName;

        [Column("inst_ownership_id")]
        public int? OwnershipId { get; set; }  // Added ? for nullable

        [ForeignKey("OwnershipId")]
        public InstOwnership? Ownership { get; set; }

        [NotMapped]
        public string? OwnershipType => Ownership?.InstOwnershipType;

        [Column("photos")]
        public string? Photos { get; set; }

        [Column("classes_levels")]
        public string? ClassesLevels { get; set; }

        [Column("fees_structure")]
        public string? FeesStructure { get; set; }

        [Column("syllabus_affilliation")]
        public string? SyllabusAffiliation { get; set; }

        [Column("keywords")]
        public string? Keyword { get; set; }

        [Column("meta_description")]
        public string? MetaDescription { get; set; }

        [Column("city_id")]
        public int? CityId { get; set; }

        [Column("syllabus_id")]
        public int? SyllabusId { get; set; }

        public City? City { get; set; }
        public Syllabus? Syllabus { get; set; }

        public ICollection<SchoolSyllabus>? SchoolSyllabuses { get; set; }

        [Column("admcriteria")]
        public string? AdmissionCriteria { get; set; }

        [Column("extracurricular")]
        public string? Extracurricular { get; set; }

        [Column("inst_timings")]
        public string? InstTimings { get; set; }

        [Column("age_group")]
        public string? AgeGroup { get; set; }

        [Column("transport")]
        public string? Transport { get; set; }

        [Column("listing_rank")]
        public int? ListingRank { get; set; }

        [Column("is_sponsored")]
        public bool IsSponsored { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;  // Added for consistency
    }
}