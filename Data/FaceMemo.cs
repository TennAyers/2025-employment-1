using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 追加
using System.Text.Json.Serialization; // 追加

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
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? SocialMediaHandle { get; set; }

        [Required]
        public string? FaceDescriptorJson { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ConversationLog> ConversationLogs { get; set; } = new List<ConversationLog>();

        // データの所有者である組織 (外部キー)
        // これが同じユーザー同士なら、このFaceMemoを閲覧できる
        public Guid OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        [JsonIgnore]
        public Organization? Organization { get; set; }

        // (オプション) 誰が作成したかを記録だけしたい場合
        public int? CreatedByUserId { get; set; }
    }
}