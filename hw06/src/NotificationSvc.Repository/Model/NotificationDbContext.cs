using Microsoft.EntityFrameworkCore;

namespace NotificationSvc.Repository.Model
{
    public class NotificationDbContext : DbContext
    {
        public DbSet<NotificationEntity> Notifications { get; set; }
        
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NotificationEntity>()
                .HasKey(ae => ae.Id);

            modelBuilder.Entity<NotificationEntity>()
                .Property(ae => ae.NotificationId)
                .IsRequired();

            modelBuilder.Entity<NotificationEntity>()
                .HasIndex(ae => ae.NotificationId)
                .IsUnique();

            modelBuilder.Entity<NotificationEntity>()
                .Property(ae => ae.UserId)
                .IsRequired();
        }
    }
}
