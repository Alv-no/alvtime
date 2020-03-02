using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class AlvTimeDBContext : DbContext
    {
        public AlvTimeDBContext()
        {
        }

        public AlvTimeDBContext(DbContextOptions<AlvTimeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<HourRate> HourRate { get; set; }
        public virtual DbSet<Hours> Hours { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Task> Task { get; set; }
        public virtual DbSet<TaskFavorites> TaskFavorites { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasOne(d => d.CustomerNavigation)
                    .WithMany(p => p.Project)
                    .HasForeignKey(d => d.Customer)
                    .HasConstraintName("FK_Project_Customer");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasOne(d => d.ProjectNavigation)
                    .WithMany(p => p.Task)
                    .HasForeignKey(d => d.Project)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_Project");
            });
        }
    }
}
