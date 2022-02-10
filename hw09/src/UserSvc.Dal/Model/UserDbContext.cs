using Microsoft.EntityFrameworkCore;
using Common.Helpers;
using Common.Model.UserSvc;

namespace UserSvc.Dal.Model;

public class UserDbContext : DbContext
{
    public const string SCHEMA = "user_svc";
    
    public DbSet<UserEntity> Users { get; set; }

    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA);

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
                    UserId = Generator.GenerateId(),
                    Username = "admin",
                    Email = "admin@admin",
                    PasswordHash = Hasher.CalculateHash("admin"),
                    Roles = UserRoles.Employee
                });
    }
}