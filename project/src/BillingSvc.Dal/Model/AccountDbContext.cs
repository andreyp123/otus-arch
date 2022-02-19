using Microsoft.EntityFrameworkCore;

namespace BillingSvc.Dal.Model
{
    public class AccountDbContext : DbContext
    {
        public const string SCHEMA = "billing_svc";
        
        public DbSet<AccountEntity> Accounts { get; set; }
        
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SCHEMA);

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
