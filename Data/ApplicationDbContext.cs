using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Models;

namespace _2025_employment_1.Data
{
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

            // 初期データ (Organization) 10件の投入
            modelBuilder.Entity<Organization>().HasData(
                new Organization { Id = Guid.Parse("d8591a27-6350-4d4f-9818-701385055051"), Name = "本社開発部" },
                new Organization { Id = Guid.Parse("2656f481-2b7e-46bb-b778-433555207212"), Name = "大阪支店" },
                new Organization { Id = Guid.Parse("a0f3d4c4-7230-4596-a447-735952331584"), Name = "東京営業所" },
                new Organization { Id = Guid.Parse("e198084e-2868-4503-b0e6-990747424422"), Name = "名古屋センター" },
                new Organization { Id = Guid.Parse("83907106-e752-4a00-9833-286c0716c514"), Name = "福岡ラボ" },
                new Organization { Id = Guid.Parse("c3182512-8806-444a-953e-5d15c8172901"), Name = "札幌サテライト" },
                new Organization { Id = Guid.Parse("f5e1b2a9-0d8c-4e6f-924b-3d7a8b5c9e0d"), Name = "海外事業部" },
                new Organization { Id = Guid.Parse("9d7f6c3a-1b2e-48a5-bc9d-4e5f6a7b8c9d"), Name = "品質管理課" },
                new Organization { Id = Guid.Parse("b2a4c6d8-9e0f-41a3-8c5d-7b9e0f2a4c6d"), Name = "人事総務部" },
                new Organization { Id = Guid.Parse("4e6f8a0b-2c4d-6e8f-0a2b-4c6d8e0f2a4b"), Name = "第2開発室" }
            );
        }
    }
}