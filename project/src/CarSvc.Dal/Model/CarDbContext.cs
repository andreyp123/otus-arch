using Microsoft.EntityFrameworkCore;

namespace CarSvc.Dal.Model;

public class CarDbContext : DbContext
{
    public const string SCHEMA = "car_svc";
    
    public DbSet<CarEntity> Cars { get; set; }
    public DbSet<CarRentEntity> CarRents { get; set; }
    
    public CarDbContext(DbContextOptions<CarDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SCHEMA);

        BuildCars(modelBuilder);
        BuildCarRents(modelBuilder);
    }

    private void BuildCars(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarEntity>()
            .HasKey(ce => ce.Id);

        modelBuilder.Entity<CarEntity>()
            .Property(ce => ce.CarId)
            .IsRequired();

        modelBuilder.Entity<CarEntity>()
            .HasIndex(ce => ce.CarId)
            .IsUnique();
    }

    private void BuildCarRents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CarRentEntity>()
            .HasKey(cre => cre.Id);

        modelBuilder.Entity<CarRentEntity>()
            .HasOne(cre => cre.Car)
            .WithMany(ce => ce.CarRents)
            .HasForeignKey(cre => cre.CarId);
    }
}
