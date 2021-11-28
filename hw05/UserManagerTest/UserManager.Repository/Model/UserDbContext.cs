using Microsoft.EntityFrameworkCore;
using UserManager.Common.Helpers;
using UserManager.Common.Model;

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
                .Property(ue => ue.UserId)
                .IsRequired();

            modelBuilder.Entity<UserEntity>()
                .HasIndex(ue => ue.UserId)
                .IsUnique();

            modelBuilder.Entity<UserEntity>()
                .Property(ue => ue.Username)
                .IsRequired();

            modelBuilder.Entity<UserEntity>()
                .HasIndex(ue => ue.Username)
                .IsUnique();

            modelBuilder.Entity<UserEntity>()
                .HasData(
                    new UserEntity
                    {
                        Id = -1,
                        UserId = IdGenerator.Generate(),
                        Username = "admin",
                        Email = "admin@admin",
                        PasswordHash = Hasher.CalculateHash("admin"),
                        Roles = UserRoles.Admin
                    });
        }
    }
}
