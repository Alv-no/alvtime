using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class AlvTime_dbContext : DbContext
    {
        public AlvTime_dbContext()
        {
        }

        public AlvTime_dbContext(DbContextOptions<AlvTime_dbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccessTokens> AccessTokens { get; set; }
        public virtual DbSet<AssociatedTasks> AssociatedTasks { get; set; }
        public virtual DbSet<CompensationRate> CompensationRate { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<EarnedOvertime> EarnedOvertime { get; set; }
        public virtual DbSet<EmploymentRate> EmploymentRate { get; set; }
        public virtual DbSet<HourRate> HourRate { get; set; }
        public virtual DbSet<Hours> Hours { get; set; }
        public virtual DbSet<PaidOvertime> PaidOvertime { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<RegisteredFlex> RegisteredFlex { get; set; }
        public virtual DbSet<Task> Task { get; set; }
        public virtual DbSet<TaskFavorites> TaskFavorites { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<VDataDump> VDataDump { get; set; }
        public virtual DbSet<VacationDaysEarnedOverride> VacationDaysEarnedOverride { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccessTokens>(entity =>
            {
                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AccessTokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccessTokens_User");
            });

            modelBuilder.Entity<AssociatedTasks>(entity =>
            {
                entity.Property(e => e.EndDate).HasDefaultValueSql("('')");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.AssociatedTasks)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssociatedTasks_Task");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AssociatedTasks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AssociatedTasks_User");
            });

            modelBuilder.Entity<CompensationRate>(entity =>
            {
                entity.Property(e => e.Value).HasColumnType("decimal(4, 2)");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.CompensationRate)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CompensationRate_Task");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.ContactEmail)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ContactPerson)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.ContactPhone)
                    .IsRequired()
                    .HasMaxLength(12)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.InvoiceAddress)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<EarnedOvertime>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.Date, e.CompensationRate }, "UC_EarnedOvertime")
                    .IsUnique();

                entity.Property(e => e.CompensationRate).HasColumnType("decimal(4, 2)");

                entity.Property(e => e.Value).HasColumnType("decimal(7, 2)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EarnedOvertime)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EarnedOvertime_User");
            });

            modelBuilder.Entity<EmploymentRate>(entity =>
            {
                entity.Property(e => e.Rate).HasColumnType("decimal(7, 2)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmploymentRate)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmploymentRate_User");
            });

            modelBuilder.Entity<HourRate>(entity =>
            {
                entity.Property(e => e.Rate).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.HourRate)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HourRate_Task");
            });

            modelBuilder.Entity<Hours>(entity =>
            {
                entity.ToTable("hours");

                entity.HasIndex(e => new { e.Date, e.TaskId, e.User }, "UC_hours_user_task")
                    .IsUnique();

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Value).HasColumnType("decimal(6, 2)");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Hours)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hours_Task");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.Hours)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_hours_User");
            });

            modelBuilder.Entity<PaidOvertime>(entity =>
            {
                entity.HasIndex(e => new { e.User, e.Date, e.CompensationRate }, "UC_PaidOvertime")
                    .IsUnique();

                entity.Property(e => e.CompensationRate).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.HoursAfterCompRate).HasColumnType("decimal(6, 2)");

                entity.Property(e => e.HoursBeforeCompRate).HasColumnType("decimal(6, 2)");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.PaidOvertime)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PaidOvertime_User");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.CustomerNavigation)
                    .WithMany(p => p.Project)
                    .HasForeignKey(d => d.Customer)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Project_Customer");
            });

            modelBuilder.Entity<RegisteredFlex>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.Date, e.CompensationRate }, "UC_RegisteredFlex")
                    .IsUnique();

                entity.Property(e => e.CompensationRate).HasColumnType("decimal(4, 2)");

                entity.Property(e => e.Value).HasColumnType("decimal(7, 2)");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RegisteredFlex)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RegisteredFlex_User");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.ProjectNavigation)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.Project)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_Project");
            });

            modelBuilder.Entity<TaskFavorites>(entity =>
            {
                entity.HasOne(d => d.Task)
                    .WithMany(p => p.TaskFavorites)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskFavorites_Task");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TaskFavorites)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskFavorites_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("('')");
            });

            modelBuilder.Entity<VDataDump>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("V_DataDump");

                entity.Property(e => e.CustomerId).HasColumnName("customerId");

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("customerName");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Earnings)
                    .HasColumnType("decimal(17, 4)")
                    .HasColumnName("earnings");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.HourRate).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ProjectId).HasColumnName("projectID");

                entity.Property(e => e.ProjectName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("projectName");

                entity.Property(e => e.TaskId).HasColumnName("taskID");

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("taskName");

                entity.Property(e => e.UserId).HasColumnName("userID");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("userName");

                entity.Property(e => e.Value)
                    .HasColumnType("decimal(6, 2)")
                    .HasColumnName("value");
            });

            modelBuilder.Entity<VacationDaysEarnedOverride>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.Year }, "UC_VacationDaysEarnedOverride")
                    .IsUnique();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.VacationDaysEarnedOverride)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_VacationDaysEarnedOverride_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
