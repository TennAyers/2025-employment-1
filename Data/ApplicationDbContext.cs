using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Models; // Productモデルを読み込むために追加

namespace _2025_employment_1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ▼▼▼ この行を追加 ▼▼▼
        // Productモデルを扱うためのDbSet。テーブル名は「Products」になります。
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}