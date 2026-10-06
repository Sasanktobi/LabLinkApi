using Backend.Constants;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public class LabLinkDbContext:DbContext
    {
        public LabLinkDbContext(DbContextOptions<LabLinkDbContext> options):base(options){}

        public DbSet<Role> Roles{get;set;}=null!;
        public DbSet<User> Users{get;set;}=null!;
        public DbSet<Admin> Admins{get;set;}=null!;
        public DbSet<Patient> Patients{get;set;}=null!;
        public DbSet<Doctor> Doctors{get;set;}=null!;
        public DbSet<Pathologist> Pathologists{get;set;}=null!;
        public DbSet<LabTechnician> LabTechnicians{get;set;}=null!;
        public DbSet<Phlebotomist> Phlebotomists{get;set;}=null!;
        public DbSet<Receptionist> Receptionists{get;set;}=null!;
        public DbSet<AvailabilitySlot> AvailabilitySlots{get;set;}=null!;
        public DbSet<Appointment> Appointments{get;set;}=null!;
        public DbSet<Test> Tests{get;set;}=null!;
        public DbSet<TestPanel> TestPanels{get;set;}=null!;
        public DbSet<TestPanelTest> TestPanelTests{get;set;}=null!;
        public DbSet<AppointmentTest> AppointmentTests{get;set;}=null!;
        public DbSet<Specimen> Specimens{get;set;}=null!;
        public DbSet<TestResult> TestResults{get;set;}=null!;
        public DbSet<Report> Reports{get;set;}=null!;
        public DbSet<AuditLog> AuditLogs{get;set;}=null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(r => r.Name).HasMaxLength(50);
                entity.HasIndex(r => r.Name).IsUnique();

                // Fixed ids so code and clients can rely on them.
                entity.HasData(
                    new Role { RoleId = 1, Name = RoleNames.Admin },
                    new Role { RoleId = 2, Name = RoleNames.Patient },
                    new Role { RoleId = 3, Name = RoleNames.Doctor },
                    new Role { RoleId = 4, Name = RoleNames.Pathologist },
                    new Role { RoleId = 5, Name = RoleNames.LabTechnician },
                    new Role { RoleId = 6, Name = RoleNames.Phlebotomist },
                    new Role { RoleId = 7, Name = RoleNames.Receptionist });
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(256);
                entity.Property(u => u.PhoneNo).HasMaxLength(20);
                entity.Property(u => u.PasswordHash).HasMaxLength(500);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Role profiles: one-to-one with User.
            // WithOne() is what makes these 1:1 - by convention EF would map them as one-to-many.

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.User)
                .WithOne(u => u.Admin)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Patient)
                      .HasForeignKey<Patient>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.Gender).HasMaxLength(20);
                entity.Property(p => p.BloodGroup).HasMaxLength(10);
                entity.Property(p => p.City).HasMaxLength(100);
                entity.Property(p => p.State).HasMaxLength(100);
                entity.Property(p => p.PostalCode).HasMaxLength(20);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasOne(d => d.User)
                      .WithOne(u => u.Doctor)
                      .HasForeignKey<Doctor>(d => d.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(d => d.Specialization).HasMaxLength(150);
                entity.Property(d => d.Qualification).HasMaxLength(150);
                entity.Property(d => d.LicenseNumber).HasMaxLength(50);
                entity.HasIndex(d => d.LicenseNumber).IsUnique();
            });

            modelBuilder.Entity<Pathologist>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Pathologist)
                      .HasForeignKey<Pathologist>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.Specialization).HasMaxLength(150);
                entity.Property(p => p.Qualification).HasMaxLength(150);
                entity.Property(p => p.LicenseNumber).HasMaxLength(50);
                entity.HasIndex(p => p.LicenseNumber).IsUnique();
            });

            modelBuilder.Entity<LabTechnician>(entity =>
            {
                entity.HasOne(l => l.User)
                      .WithOne(u => u.LabTechnician)
                      .HasForeignKey<LabTechnician>(l => l.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(l => l.Department).HasMaxLength(100);
                entity.Property(l => l.ShiftTiming).HasMaxLength(50);
            });

            modelBuilder.Entity<Phlebotomist>(entity =>
            {
                entity.HasOne(p => p.User)
                      .WithOne(u => u.Phlebotomist)
                      .HasForeignKey<Phlebotomist>(p => p.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(p => p.ServiceZone).HasMaxLength(100);
                entity.HasIndex(p => p.ServiceZone);
            });

            modelBuilder.Entity<Receptionist>(entity =>
            {
                entity.HasOne(r => r.User)
                      .WithOne(u => u.Receptionist)
                      .HasForeignKey<Receptionist>(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.ShiftTiming).HasMaxLength(50);
                entity.Property(r => r.BranchLocation).HasMaxLength(100);
            });

            // Scheduling

            modelBuilder.Entity<AvailabilitySlot>(entity =>
            {
                entity.HasOne(s => s.User)
                      .WithMany(u => u.AvailabilitySlots)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(s => s.StartTime).HasMaxLength(10);
                entity.Property(s => s.EndTime).HasMaxLength(10);
                entity.Property(s => s.ProviderType).HasMaxLength(30);
                entity.Property(s => s.RowVersion).IsRowVersion();
                entity.HasIndex(s => new { s.UserId, s.SlotDate });
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasOne(a => a.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PatientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(a => a.DoctorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Phlebotomist)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(a => a.PhlebotomistId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.AvailabilitySlot)
                      .WithMany(s => s.Appointments)
                      .HasForeignKey(a => a.AvailabilitySlotId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.BookedByUser)
                      .WithMany(u => u.AppointmentsBooked)
                      .HasForeignKey(a => a.BookedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.ScheduleTimeSlot).HasMaxLength(30);
                entity.Property(a => a.AppointmentType).HasMaxLength(30);
                entity.Property(a => a.Status).HasMaxLength(30);
                entity.Property(a => a.BookedBy).HasMaxLength(30);
                entity.HasIndex(a => new { a.ScheduleDate, a.Status });
                entity.HasIndex(a => a.PatientId);
            });

            // Test catalogue

            modelBuilder.Entity<Test>(entity =>
            {
                entity.Property(t => t.Name).HasMaxLength(150);
                entity.HasIndex(t => t.Name).IsUnique();
                entity.Property(t => t.Code).HasMaxLength(30);
                entity.HasIndex(t => t.Code).IsUnique();
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.SampleType).HasMaxLength(50);
                entity.Property(t => t.Price).HasPrecision(18, 2);
                entity.Property(t => t.Unit).HasMaxLength(30);
                entity.Property(t => t.ReferenceRangeLow).HasMaxLength(50);
                entity.Property(t => t.ReferenceRangeHigh).HasMaxLength(50);
            });

            modelBuilder.Entity<TestPanel>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2);
                entity.Property(p => p.Name).HasMaxLength(150);
                entity.HasIndex(p => p.Name).IsUnique();
            });

            // Which tests make up a panel (many-to-many, composite key)

            modelBuilder.Entity<TestPanelTest>(entity =>
            {
                entity.HasKey(pt => new { pt.TestPanelId, pt.TestId });

                entity.HasOne(pt => pt.TestPanel)
                      .WithMany(p => p.TestPanelTests)
                      .HasForeignKey(pt => pt.TestPanelId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pt => pt.Test)
                      .WithMany(t => t.TestPanelTests)
                      .HasForeignKey(pt => pt.TestId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Which tests were actually ordered on an appointment

            modelBuilder.Entity<AppointmentTest>(entity =>
            {
                entity.HasOne(at => at.Appointment)
                      .WithMany(a => a.AppointmentTests)
                      .HasForeignKey(at => at.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(at => at.Test)
                      .WithMany(t => t.AppointmentTests)
                      .HasForeignKey(at => at.TestId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(at => at.TestPanel)
                      .WithMany(p => p.AppointmentTests)
                      .HasForeignKey(at => at.TestPanelId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(at => at.PriceAtOrder).HasPrecision(18, 2);
                entity.Property(at => at.Status).HasMaxLength(30);
                entity.HasIndex(at => new { at.AppointmentId, at.TestId }).IsUnique();
            });

            // Collection and results

            modelBuilder.Entity<Specimen>(entity =>
            {
                entity.HasOne(s => s.Appointment)
                      .WithMany(a => a.Specimens)
                      .HasForeignKey(s => s.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.CollectedByUser)
                      .WithMany(u => u.SpecimensCollected)
                      .HasForeignKey(s => s.CollectedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(s => s.SpecimenName).HasMaxLength(100);
                entity.Property(s => s.Status).HasMaxLength(30);
                entity.Property(s => s.RejectionReason).HasMaxLength(500);
            });

            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasOne(r => r.Specimen)
                      .WithMany(s => s.TestResults)
                      .HasForeignKey(r => r.SpecimenId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Test)
                      .WithMany(t => t.TestResults)
                      .HasForeignKey(r => r.TestId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.EnteredByLabTechnician)
                      .WithMany(l => l.TestResultsEntered)
                      .HasForeignKey(r => r.EnteredByLabTechnicianId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.ResultValue).HasMaxLength(100);
                entity.Property(r => r.Flag).HasMaxLength(20);
                entity.Property(r => r.Remarks).HasMaxLength(500);
                entity.HasIndex(r => new { r.SpecimenId, r.TestId }).IsUnique();
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.Appointment)
                      .WithOne(a => a.Report)
                      .HasForeignKey<Report>(r => r.AppointmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.ValidatedByPathologist)
                      .WithMany(p => p.ReportsValidated)
                      .HasForeignKey(r => r.ValidatedByPathologistId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(r => r.PdfFilepath).HasMaxLength(500);
                entity.Property(r => r.Status).HasMaxLength(30);
                entity.Property(r => r.PathologistRemarks).HasMaxLength(1000);
            });

            // Auditing

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasOne(a => a.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(a => a.EntityName).HasMaxLength(100);
                entity.Property(a => a.Action).HasMaxLength(50);
                entity.HasIndex(a => new { a.EntityName, a.EntityId });
            });
        }
    }
}
