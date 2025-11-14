using Microsoft.EntityFrameworkCore;
using CAaR.Models;

namespace CAaR.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<HelpRequest> HelpRequests { get; set; }
        public DbSet<RoleRequest> RoleRequests { get; set; }
        public DbSet<ImageRequest> ImageRequests { get; set; }
        public DbSet<NameRequest> NameRequests { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Result>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID);

            modelBuilder.Entity<HelpRequest>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserID);

            modelBuilder.Entity<RoleRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID);

            modelBuilder.Entity<ImageRequest>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID);

            modelBuilder.Entity<NameRequest>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserID);

            modelBuilder.Entity<Message>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID);
        }
    }
}
