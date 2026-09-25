using Microsoft.EntityFrameworkCore;
using MedicalAppointment.Shared.Entities;

namespace MedicalAppointment.Shared.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options) { }

        // ═══ 10 DbSets ═══
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<DoctorDayOff> DoctorDayOffs { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ═══ ROLE ═══
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.Code).IsUnique();
                entity.Property(r => r.Code).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Name).IsRequired().HasMaxLength(100);

                // Seed Data
                entity.HasData(
                    new Role { Id = 1, Code = "ADMIN", Name = "Quản trị viên", Description = "Toàn quyền hệ thống" },
                    new Role { Id = 2, Code = "PATIENT", Name = "Bệnh nhân", Description = "Người dùng đặt lịch" }
                );
            });

            // ═══ USER ═══
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.IsActive).HasDefaultValue(true);

                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══ DEPARTMENT ═══
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasIndex(d => d.Name).IsUnique();
                entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
                entity.Property(d => d.IsActive).HasDefaultValue(true);
            });

            // ═══ ROOM ═══
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.RoomNumber).IsRequired().HasMaxLength(20);
                entity.Property(r => r.IsActive).HasDefaultValue(true);

                entity.HasOne(r => r.Department)
                    .WithMany(d => d.Rooms)
                    .HasForeignKey(r => r.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══ DOCTOR ═══
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.FullName).IsRequired().HasMaxLength(100);
                entity.Property(d => d.Qualification).HasMaxLength(100);
                entity.Property(d => d.ConsultationFee).HasColumnType("decimal(18,2)");
                entity.Property(d => d.IsActive).HasDefaultValue(true);

                entity.HasOne(d => d.Department)
                    .WithMany(dep => dep.Doctors)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Room)
                    .WithMany(r => r.Doctors)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ═══ DOCTOR SCHEDULE ═══
            modelBuilder.Entity<DoctorSchedule>(entity =>
            {
                entity.HasKey(ds => ds.Id);
                entity.Property(ds => ds.MaxPatients).HasDefaultValue(20);

                entity.HasOne(ds => ds.Doctor)
                    .WithMany(d => d.Schedules)
                    .HasForeignKey(ds => ds.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ═══ DOCTOR DAY OFF ═══
            modelBuilder.Entity<DoctorDayOff>(entity =>
            {
                entity.HasKey(ddo => ddo.Id);
                entity.Property(ddo => ddo.Date).HasColumnType("date");
                entity.Property(ddo => ddo.Reason).HasMaxLength(200);

                entity.HasOne(ddo => ddo.Doctor)
                    .WithMany(d => d.DayOffs)
                    .HasForeignKey(ddo => ddo.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ═══ PATIENT PROFILE ═══
            modelBuilder.Entity<PatientProfile>(entity =>
            {
                entity.HasKey(pp => pp.Id);
                entity.Property(pp => pp.FullName).IsRequired().HasMaxLength(100);
                entity.Property(pp => pp.Gender).IsRequired().HasMaxLength(10);
                entity.Property(pp => pp.Ethnicity).HasMaxLength(50);
                entity.Property(pp => pp.Occupation).HasMaxLength(100);
                entity.Property(pp => pp.Relationship).IsRequired().HasMaxLength(50);
                entity.Property(pp => pp.IdentityCardNumber).HasMaxLength(20);
                entity.Property(pp => pp.HealthInsuranceNumber).HasMaxLength(50);

                entity.HasOne(pp => pp.User)
                    .WithMany(u => u.PatientProfiles)
                    .HasForeignKey(pp => pp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ═══ CART ITEM ═══
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);
                entity.Property(ci => ci.TimeSlot).IsRequired().HasMaxLength(20);

                entity.HasOne(ci => ci.User)
                    .WithMany(u => u.CartItems)
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Doctor)
                    .WithMany(d => d.CartItems)
                    .HasForeignKey(ci => ci.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ci => ci.PatientProfile)
                    .WithMany(pp => pp.CartItems)
                    .HasForeignKey(ci => ci.PatientProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══ ORDER ═══
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(o => o.Status).IsRequired().HasMaxLength(20);
                entity.Property(o => o.CancellationReason).HasMaxLength(500);
                entity.Property(o => o.TransactionId).HasMaxLength(100);
                entity.Property(o => o.PaymentMethod).IsRequired().HasMaxLength(20);
                entity.Property(o => o.PaymentStatus).IsRequired().HasMaxLength(20);

                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ═══ ORDER DETAIL ═══
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(od => od.Id);
                entity.Property(od => od.Fee).HasColumnType("decimal(18,2)");
                entity.Property(od => od.TimeSlot).IsRequired().HasMaxLength(20);
                entity.Property(od => od.Status).IsRequired().HasMaxLength(20);
                entity.Property(od => od.QueueNumber).IsRequired();

                entity.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(od => od.Doctor)
                    .WithMany(d => d.OrderDetails)
                    .HasForeignKey(od => od.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(od => od.PatientProfile)
                    .WithMany(pp => pp.OrderDetails)
                    .HasForeignKey(od => od.PatientProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
