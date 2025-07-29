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

    }
}