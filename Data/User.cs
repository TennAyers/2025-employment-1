using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace _2025_employment_1.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; } // ログインID代わり

        [Required]
        public string PasswordHash { get; set; } // パスワード（ハッシュ化して保存推奨）

        public string FullName { get; set; }
        public Guid? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        [JsonIgnore]
        public Organization? Organization { get; set; }
    }
}