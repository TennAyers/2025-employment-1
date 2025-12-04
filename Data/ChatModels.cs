using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using _2025_employment_1.Models; // ★これで User や Organization が見つかります

namespace _2025_employment_1.Data
{
    // チャットルーム（1対1 または グループ）
    public class ChatRoom
    {
        public int Id { get; set; } // ルームIDは管理しやすいよう int のままでOK（URL等で使いやすいため）
        
        [Required]
        public string Name { get; set; } = "新規トークルーム";

        // 所属ID (Guidに対応)
        public Guid OrganizationId { get; set; } 
        public Organization? Organization { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    // チャットルームのメンバー
    public class ChatMember
    {
        public int Id { get; set; }
        
        public int ChatRoomId { get; set; }
        public ChatRoom? ChatRoom { get; set; }

        // ユーザーID (Guidに対応)
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }

    // メッセージ本体
    public class ChatMessage
    {
        public int Id { get; set; }
        
        public int ChatRoomId { get; set; }
        public ChatRoom? ChatRoom { get; set; }

        // 送信者ID (Guidに対応)
        public Guid SenderId { get; set; }
        public User? Sender { get; set; }

        [Required]
        public string Content { get; set; } = "";

        public DateTime SentAt { get; set; } = DateTime.Now;
        
        public bool IsRead { get; set; } = false;
    }
}