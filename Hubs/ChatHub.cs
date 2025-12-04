using Microsoft.AspNetCore.SignalR;
using _2025_employment_1.Data;

namespace _2025_employment_1.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // 特定のルームに参加する
        public async Task JoinRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        // メッセージを送信する
        public async Task SendMessage(string roomId, string user, string message)
        {
            // クライアント（画面）の "ReceiveMessage" 関数を呼び出す
            await Clients.Group(roomId).SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"));
        }
    }
}