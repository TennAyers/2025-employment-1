using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // API返却時のループ防止用

namespace _2025_employment_1.Models
{
    public class ConversationLog
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = ""; // 話した内容

        public DateTime Date { get; set; } = DateTime.Now; // 日時

        // どの人物の会話か (外部キー)
        public int FaceMemoId { get; set; }

        [ForeignKey("FaceMemoId")]
        [JsonIgnore] // APIで返すときに無限ループしないように除外
        public FaceMemo? FaceMemo { get; set; }
    }
}