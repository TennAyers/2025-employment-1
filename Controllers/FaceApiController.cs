using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Data;
using _2025_employment_1.Models;
using System.Text.Json;

namespace _2025_employment_1.Controllers
{
    [Route("api/face")]
    [ApiController]
    public class FaceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FaceApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. 顔の識別 (POST: api/face/identify)
        [HttpPost("identify")]
        public async Task<IActionResult> Identify([FromBody] IdentifyRequest request)
        {
            try 
            {
                var inputDescriptor = JsonSerializer.Deserialize<float[]>(request.Descriptor);
                if (inputDescriptor == null) return BadRequest("Invalid descriptor");

                var allFaces = await _context.FaceMemos.Include(f => f.ConversationLogs).ToListAsync();
                
                FaceMemo? bestMatch = null;
                double minDistance = 0.6; // 閾値 (0.6以下なら同一人物とみなす)

                foreach (var face in allFaces)
                {
                    var storedDescriptor = JsonSerializer.Deserialize<float[]>(face.FaceDescriptorJson);
                    if (storedDescriptor != null)
                    {
                        var distance = EuclideanDistance(inputDescriptor, storedDescriptor);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            bestMatch = face;
                        }
                    }
                }

                if (bestMatch != null)
                {
                    // 直近の会話ログを3件取得
                    var logs = await _context.ConversationLogs
                        .Where(l => l.FaceMemoId == bestMatch.Id)
                        .OrderByDescending(l => l.Date)
                        .Take(3)
                        .Select(l => new { l.Date, l.Content })
                        .ToListAsync();

                    return Ok(new { 
                        success = true, 
                        id = bestMatch.Id,
                        name = bestMatch.Name, 
                        affiliation = bestMatch.Affiliation, 
                        notes = bestMatch.Notes,
                        logs = logs // 会話ログも返す
                    });
                }
                
                return Ok(new { success = false, message = "Unknown" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        // 2. 顔の登録 (POST: api/face/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] FaceMemo model)
        {
            _context.FaceMemos.Add(model);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, name = model.Name });
        }

        // 3. 会話ログの追加 (POST: api/face/add_log)
        [HttpPost("add_log")]
        public async Task<IActionResult> AddLog([FromBody] LogRequest request)
        {
            var face = await _context.FaceMemos.FindAsync(request.FaceId);
            if (face == null) return NotFound("User not found");

            var log = new ConversationLog
            {
                FaceMemoId = request.FaceId,
                Content = request.Content,
                Date = DateTime.Now
            };

            _context.ConversationLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        // 距離計算ロジック
        private double EuclideanDistance(float[] d1, float[] d2)
        {
            double sum = 0.0;
            for (int i = 0; i < d1.Length; i++) sum += Math.Pow(d1[i] - d2[i], 2);
            return Math.Sqrt(sum);
        }
    }

    // リクエスト用クラス
    public class IdentifyRequest { public string Descriptor { get; set; } }
    public class LogRequest { public int FaceId { get; set; } public string Content { get; set; } }
}