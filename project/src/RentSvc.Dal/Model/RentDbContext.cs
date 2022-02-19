using Microsoft.EntityFrameworkCore;

namespace RentSvc.Dal.Model
{
    public class RentDbContext : DbContext
    {
        public const string SCHEMA = "rent_svc";
        
        public DbSet<RentEntity> Rents { get; set; }
        public DbSet<RequestEntity> Requests { get; set; }

        public RentDbContext(DbContextOptions<RentDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(SCHEMA);

            BuildRents(modelBuilder);
            BuildRequests(modelBuilder);
        }

        private void BuildRents(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RentEntity>()
                .HasKey(oe => oe.Id);

            modelBuilder.Entity<RentEntity>()
                .Property(oe => oe.RentId)
                .IsRequired();

            modelBuilder.Entity<RentEntity>()
                .HasIndex(oe => oe.RentId)
                .IsUnique();

            modelBuilder.Entity<RentEntity>()
                .Property(oe => oe.UserId)
                .IsRequired();

            modelBuilder.Entity<RentEntity>()
                .Property(oe => oe.CarId)
                .IsRequired();

            modelBuilder.Entity<RentEntity>()
                .Property(oe => oe.State)
                .IsRequired();
        }

        private void BuildRequests(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RequestEntity>()
                .HasKey(re => new {re.Id, re.Name, re.Date});
        }
    }
}
