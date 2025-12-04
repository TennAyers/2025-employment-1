using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Data;
using Microsoft.AspNetCore.Authorization;
using _2025_employment_1.Models;
using System.Security.Claims;

namespace _2025_employment_1.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        private User? GetCurrentUser()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId)) 
            {
                return null;
            }
            return _context.Users
                .Include(u => u.Organization)
                .FirstOrDefault(u => u.Id == userId);
        }

        // ==========================================
        //  通常画面用（今回使いませんが残しておきます）
        // ==========================================
        public async Task<IActionResult> Index()
        {
            /* 既存コードそのまま */
            return View(); 
        }

        // ==========================================
        //  ★追加: Ajax用アクション
        // ==========================================

        // 1. チャット一覧の「部品」を返す
        [HttpGet]
        public async Task<IActionResult> GetListPartial()
        {
            var user = GetCurrentUser();
            if (user == null || user.OrganizationId == null) return Unauthorized();

            var myRooms = await _context.ChatRooms
                .Include(r => r.Members)
                .Where(r => r.OrganizationId == user.OrganizationId && r.Members.Any(m => m.UserId == user.Id))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.OrgUsers = await _context.Users
                .Where(u => u.OrganizationId == user.OrganizationId && u.Id != user.Id)
                .ToListAsync();

            ViewBag.CurrentUserId = user.Id;

            // 部分ビューを返す
            return PartialView("_ChatList", myRooms);
        }

        // 2. 特定のルームの「部品」を返す
        [HttpGet]
        public async Task<IActionResult> GetRoomPartial(int id)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized();

            var room = await _context.ChatRooms
                .Include(r => r.Messages).ThenInclude(m => m.Sender)
                .Include(r => r.Members)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null || room.OrganizationId != user.OrganizationId || !room.Members.Any(m => m.UserId == user.Id))
            {
                return NotFound();
            }

            ViewBag.CurrentUserId = user.Id;
            ViewBag.CurrentUserName = user.FullName ?? user.Email;
            
            return PartialView("_ChatRoom", room);
        }

        // 3. ルーム作成 (JSONを返す)
        [HttpPost]
        public async Task<IActionResult> CreateRoomAjax(string roomName, List<Guid> targetUserIds)
        {
            var user = GetCurrentUser();
            if (user == null || user.OrganizationId == null) return Unauthorized();

            var room = new ChatRoom
            {
                Name = roomName,
                OrganizationId = user.OrganizationId.Value,
                CreatedAt = DateTime.Now
            };

            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();

            _context.ChatMembers.Add(new ChatMember { ChatRoomId = room.Id, UserId = user.Id });
            foreach (var targetId in targetUserIds)
            {
                _context.ChatMembers.Add(new ChatMember { ChatRoomId = room.Id, UserId = targetId });
            }
            await _context.SaveChangesAsync();

            // 作成したルームIDをJSONで返す
            return Json(new { success = true, roomId = room.Id });
        }

        // 4. メッセージ保存 (既存のSaveMessageを使用しても良いですが、念のため確認)
        [HttpPost]
        public async Task<IActionResult> SaveMessage(int roomId, string content)
        {
            var user = GetCurrentUser();
            if (user == null) return Unauthorized();

            var msg = new ChatMessage
            {
                ChatRoomId = roomId,
                SenderId = user.Id,
                Content = content,
                SentAt = DateTime.Now
            };
            _context.ChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}