using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace _2025_employment_1.Models
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } 

        // ナビゲーションプロパティ
        public List<User> Users { get; set; } = new List<User>();
        public List<FaceMemo> FaceMemos { get; set; } = new List<FaceMemo>();
    }
}