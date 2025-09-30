using System.ComponentModel.DataAnnotations; // This line is needed for [Required]

namespace _2025_employment_1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}