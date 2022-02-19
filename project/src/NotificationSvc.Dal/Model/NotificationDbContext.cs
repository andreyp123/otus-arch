using Microsoft.EntityFrameworkCore;

namespace NotificationSvc.Dal.Model
{
    public class NotificationDbContext : DbContext
    {
        public const string SCHEMA = "notification_svc";
        
        public DbSet<NotificationEntity> Notifications { get; set; }
        
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SCHEMA);

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
