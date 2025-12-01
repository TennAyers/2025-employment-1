using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Models;

namespace _2025_employment_1.Data
{
    // ★修正: IdentityDbContext ではなく DbContext を継承します
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<FaceMemo> FaceMemos { get; set; }
        public DbSet<ConversationLog> ConversationLogs { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 初期データの投入設定
            modelBuilder.Entity<Organization>().HasData(
                new Organization 
                { 
                    Id = 1, 
                    Name = "初期設定の組織" 
                }
            );
        }
    }
}