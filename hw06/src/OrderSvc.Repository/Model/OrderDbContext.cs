using Microsoft.EntityFrameworkCore;

namespace OrderSvc.Repository.Model
{
    public class OrderDbContext : DbContext
    {
        public DbSet<OrderEntity> Orders { get; set; }
        
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderEntity>()
                .HasKey(oe => oe.Id);

            modelBuilder.Entity<OrderEntity>()
                .Property(oe => oe.OrderId)
                .IsRequired();

            modelBuilder.Entity<OrderEntity>()
                .HasIndex(oe => oe.OrderId)
                .IsUnique();

            modelBuilder.Entity<OrderEntity>()
                .Property(oe => oe.UserId)
                .IsRequired();

            modelBuilder.Entity<OrderEntity>()
                .Property(oe => oe.State)
                .IsRequired();
        }
    }
}
