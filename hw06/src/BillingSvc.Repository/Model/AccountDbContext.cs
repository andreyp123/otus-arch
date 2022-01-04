using Microsoft.EntityFrameworkCore;

namespace BillingSvc.Repository.Model
{
    public class AccountDbContext : DbContext
    {
        public DbSet<AccountEntity> Accounts { get; set; }
        
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AccountEntity>()
                .HasKey(ae => ae.Id);

            modelBuilder.Entity<AccountEntity>()
                .Property(ae => ae.AccountId)
                .IsRequired();

            modelBuilder.Entity<AccountEntity>()
                .HasIndex(ae => ae.AccountId)
                .IsUnique();

            modelBuilder.Entity<AccountEntity>()
                .Property(ae => ae.UserId)
                .IsRequired();
        }
    }
}
