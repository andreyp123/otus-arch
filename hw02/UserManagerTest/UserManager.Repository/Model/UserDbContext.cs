using Microsoft.EntityFrameworkCore;

namespace UserManager.Repository.Model
{
    public class UserDbContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>()
                .HasKey(ue => ue.Id);

            modelBuilder.Entity<UserEntity>()
                .HasIndex(ue => ue.UserId);
        }
    }
}
