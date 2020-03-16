using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<VDataDump> VDataDump { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=AlvTimeDB;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

            modelBuilder.Entity<VDataDump>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("V_DataDump");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
