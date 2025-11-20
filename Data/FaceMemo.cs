using System;
using System.Collections.Generic; //Listを使うために必要
using System.ComponentModel.DataAnnotations;

namespace _2025_employment_1.Models 
{
    public class FaceMemo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Affiliation { get; set; }

        public string? Notes { get; set; }

        [Required]
        public string? FaceDescriptorJson { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ConversationLog> ConversationLogs { get; set; } = new List<ConversationLog>();
    }
}