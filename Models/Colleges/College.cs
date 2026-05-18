using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolProject.Models.Lookups;

namespace SchoolProject.Models.Colleges
{
    [Table("tb_colleges")]
    public class College
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

        [Column("photos")]
        public string? Photos { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("locality_id")]
        public int? LocalityId { get; set; }

        [Column("city_id")]
        public int? CityId { get; set; }

        [Column("state_id")]
        public int? StateId { get; set; }

        [Column("pincode")]
        public string? Pincode { get; set; }

        [Column("telephone")]
        public string? Telephone { get; set; }

        [Column("mobile")]
        public string? Mobile { get; set; }

        [Column("email")]
        public string? Email { get; set; }

        [Column("email2")]
        public string? Email2 { get; set; }

        [Column("website")]
        public string? Website { get; set; }

        [Column("estd")]
        public string? Estd { get; set; }

        [Column("accreditation")]
        public string? Accreditation { get; set; }

        [Column("naac_grade")]
        public string? NaacGrade { get; set; }

        [Column("nba_accredited")]
        public bool NbaAccredited { get; set; }

        [Column("approved_by")]
        public string? ApprovedBy { get; set; }

        [Column("affiliated_to")]
        public string? AffiliatedTo { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("coed_id")]
        public int? CoedId { get; set; }

        [Column("inst_ownership_id")]
        public int? InstOwnershipId { get; set; }

        [Column("hostel_available")]
        public bool HostelAvailable { get; set; }

        [Column("transport_available")]
        public bool TransportAvailable { get; set; }

        [Column("library_available")]
        public bool LibraryAvailable { get; set; }

        [Column("sports_facilities")]
        public bool SportsFacilities { get; set; }

        [Column("wifi_campus")]
        public bool WifiCampus { get; set; }

        [Column("placement_percentage")]
        public int? PlacementPercentage { get; set; }

        [Column("avg_package_lpa")]
        public decimal? AvgPackageLpa { get; set; }

        [Column("top_recruiters")]
        public string? TopRecruiters { get; set; }

        [Column("scholarship_available")]
        public bool ScholarshipAvailable { get; set; }

        [Column("scholarship_details")]
        public string? ScholarshipDetails { get; set; }

        [Column("meta_title")]
        public string? MetaTitle { get; set; }

        [Column("meta_description")]
        public string? MetaDescription { get; set; }

        [Column("keywords")]
        public string? Keywords { get; set; }

        [Column("about_institute")]
        public string? AboutInstitute { get; set; }

        [Column("listing_rank")]
        public int? ListingRank { get; set; }

        [Column("is_sponsored")]
        public bool IsSponsored { get; set; }

        [Column("is_featured")]
        public bool IsFeatured { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_date")]
        public DateTime? CreatedDate { get; set; }

        [Column("modified_date")]
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public Locality? Locality { get; set; }
        public City? City { get; set; }
        public State? State { get; set; }
        public Coed? Coed { get; set; }
        public InstOwnership? Ownership { get; set; }
        public ICollection<CollegeCourse>? CollegeCourses { get; set; }
    }
}
