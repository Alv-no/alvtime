using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace AlvTime.Persistence.EconomyDataDBModels
{
    public partial class AlvEconomyDataContext : DbContext
    {
        public AlvEconomyDataContext()
        {
        }

        public AlvEconomyDataContext(DbContextOptions<AlvEconomyDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<EmployeeHourlySalary> EmployeeHourlySalaries { get; set; }
        public virtual DbSet<OvertimePayout> OvertimePayouts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=AlvEconomyData;User ID=sa;Password=AlvTimeTestErMoro32;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<EmployeeHourlySalary>(entity =>
            {
                entity.Property(e => e.HourlySalary).HasColumnType("decimal(18, 9)");
            });

            modelBuilder.Entity<OvertimePayout>(entity =>
            {
                entity.ToTable("OvertimePayout");

                entity.Property(e => e.TotalPayout).HasColumnType("decimal(18, 9)");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
