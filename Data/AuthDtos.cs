using System.ComponentModel.DataAnnotations;

namespace _2025_employment_1.Models 
{
    // ログイン用のデータクラス
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    // 新規登録用のデータクラス
    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string OrganizationName { get; set; } 
    }
}