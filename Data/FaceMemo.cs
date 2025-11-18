using System.ComponentModel.DataAnnotations;

// ApplicationDbContext.cs の using に合わせて名前空間を決めます
namespace _2025_employment_1.Models 
{
    public class FaceMemo
    {
        [Key] // 主キー
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } // 登録する名前 (山田太郎 など)

        public string Affiliation { get; set; } // 所属 (A株式会社 など)

        public string Notes { get; set; } // メモ (C言語の勉強会で会った など)

        [Required]

        public string FaceDescriptorJson { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}