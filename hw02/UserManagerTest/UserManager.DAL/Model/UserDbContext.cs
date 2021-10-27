using Microsoft.EntityFrameworkCore;

namespace UserManager.DAL.Model
{
    public class UserDbContext : DbContext
    {
        private readonly string _connectionString;

        public DbSet<UserEntity> Users { get; set; }

        public UserDbContext()
        {
            _connectionString = "Data Source=./news.db";
        }

        public UserDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

           // optionsBuilder.UseSqlite(_connectionString);
           // todo:
           // - postgres
           // - design time builder, parameter for cli
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>()
                .HasIndex(nie => nie.UserId);
        }
    }
}
