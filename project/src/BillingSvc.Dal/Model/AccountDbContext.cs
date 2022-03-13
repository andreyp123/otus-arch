using Microsoft.EntityFrameworkCore;

namespace BillingSvc.Dal.Model
{
    public class AccountDbContext : DbContext
    {
        public const string SCHEMA = "billing_svc";
        
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<AccountEventEntity> AccountEvents { get; set; }
        
        public AccountDbContext(DbContextOptions<AccountDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SCHEMA);
            
            BuildAccounts(modelBuilder);
            BuildAccountEvents(modelBuilder);
        }

        private void BuildAccounts(ModelBuilder modelBuilder)
        {
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

        private void BuildAccountEvents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountEventEntity>()
                .HasKey(aee => aee.Id);

            modelBuilder.Entity<AccountEventEntity>()
                .HasOne(aee => aee.Account)
                .WithMany(ae => ae.AccountEvents)
                .HasForeignKey(aee => aee.AccountId);
        }
    }
}
