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

        public DbSet<FaceMemo> FaceMemos { get; set; }
        public DbSet<ConversationLog> ConversationLogs { get; set; }
        
    }
}

