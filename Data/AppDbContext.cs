using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Models.Colleges;
using SchoolProject.Models.Courses;
using SchoolProject.Models.Lookups;

namespace SchoolProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // SCHOOLS
        public DbSet<School> Schools { get; set; }
        public DbSet<Syllabus> Syllabuses { get; set; }
        public DbSet<SeoContent> SeoContents { get; set; }
        public DbSet<Enquiry> Enquiries { get; set; }
        public DbSet<DynamicContents> DynamicContents { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<SchoolSyllabus> SchoolSyllabuses { get; set; }

        // SHARED LOOKUPS
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Locality> Localities { get; set; }
        public DbSet<Coed> Coeds { get; set; }
        public DbSet<InstOwnership> InstOwnerships { get; set; }

        // COLLEGES
        public DbSet<CourseLevel> CourseLevels { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<College> Colleges { get; set; }
        public DbSet<CollegeCourse> CollegeCourses { get; set; }
        public DbSet<CollegeEnquiry> CollegeEnquiries { get; set; }
        public DbSet<SeoContentCollege> SeoContentColleges { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // SCHOOLS CONFIG
            modelBuilder.Entity<School>()
                .HasOne(s => s.Coed)
                .WithMany(c => c.Schools)
                .HasForeignKey(s => s.CoedId);

            modelBuilder.Entity<School>()
                .HasOne(s => s.Locality)
                .WithMany(l => l.Schools)
                .HasForeignKey(s => s.LocalityId);

            // STANDARDIZED: School ownership FK uses InstOwnershipId
            modelBuilder.Entity<School>()
                .HasOne(s => s.Ownership)
                .WithMany()
                .HasForeignKey(s => s.InstOwnershipId);

            modelBuilder.Entity<SchoolSyllabus>()
                .ToTable("school_syllabuses")
                .HasIndex(ss => new { ss.InstituteId, ss.SyllabusId })
                .IsUnique();

            modelBuilder.Entity<SchoolSyllabus>()
                .HasOne(ss => ss.School)
                .WithMany(s => s.SchoolSyllabuses)
                .HasForeignKey(ss => ss.InstituteId);

            modelBuilder.Entity<SchoolSyllabus>()
                .HasOne(ss => ss.Syllabus)
                .WithMany()
                .HasForeignKey(ss => ss.SyllabusId);

            // COLLEGES CONFIG
            modelBuilder.Entity<College>()
                .HasOne(c => c.City)
                .WithMany()
                .HasForeignKey(c => c.CityId);

            modelBuilder.Entity<College>()
                .HasOne(c => c.Locality)
                .WithMany(l => l.Colleges)
                .HasForeignKey(c => c.LocalityId);

            modelBuilder.Entity<College>()
                .HasOne(c => c.Coed)
                .WithMany()
                .HasForeignKey(c => c.CoedId);

            // STANDARDIZED: College ownership FK uses InstOwnershipId
            modelBuilder.Entity<College>()
                .HasOne(c => c.Ownership)
                .WithMany()
                .HasForeignKey(c => c.InstOwnershipId);

            modelBuilder.Entity<College>()
                .HasOne(c => c.State)
                .WithMany()
                .HasForeignKey(c => c.StateId);

            modelBuilder.Entity<CollegeCourse>()
                .HasOne(cc => cc.College)
                .WithMany(c => c.CollegeCourses)
                .HasForeignKey(cc => cc.InstituteId);

            modelBuilder.Entity<CollegeCourse>()
                .HasOne(cc => cc.Course)
                .WithMany()
                .HasForeignKey(cc => cc.CourseId);

            // FIX: Use SpecializationId (int?) instead of Specialization (string) for unique index
            // This avoids CS1061 if Specialization string property is missing from compiled DLL
            modelBuilder.Entity<CollegeCourse>()
                .HasIndex(cc => new { cc.InstituteId, cc.CourseId, cc.SpecializationId })
                .IsUnique();

            modelBuilder.Entity<College>()
                .HasIndex(c => c.InstituteSlug)
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => c.CourseSlug)
                .IsUnique();

            modelBuilder.Entity<CourseCategory>()
                .HasOne(cc => cc.ParentCategory)
                .WithMany(cc => cc.SubCategories)
                .HasForeignKey(cc => cc.ParentCategoryId);

            modelBuilder.Entity<City>()
                .HasIndex(c => c.CitySlug);

            modelBuilder.Entity<Locality>()
                .HasIndex(l => l.LocalitySlug);

            modelBuilder.Entity<Specialization>()
                .HasOne(s => s.Course)
                .WithMany()
                .HasForeignKey(s => s.CourseId);

            modelBuilder.Entity<CollegeCourse>()
                .HasOne(cc => cc.SpecializationNav)
                .WithMany(s => s.CollegeCourses)
                .HasForeignKey(cc => cc.SpecializationId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AdminUser>().ToTable("tb_admin_users");
        }
    }
}
