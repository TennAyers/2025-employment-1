using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Data;
using _2025_employment_1.Models;
using System.Text.Json;
using System.Security.Claims; // ★追加: Claim取得用

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

        // ★修正: 組織ID (UUID) を取得するメソッド
        private Guid GetCurrentOrganizationId()
        {
            // ログイン時に保存したClaimから取得
            var orgIdStr = User.FindFirst("OrganizationId")?.Value;

            if (Guid.TryParse(orgIdStr, out Guid orgGuid))
            {
                return orgGuid;
            }
            
            // 取得できない場合は空のGuidを返す（必要に応じてエラー処理）
            return Guid.Empty;
        }

        // 1. 顔の識別 (POST: api/face/identify)
        [HttpPost("identify")]
        public async Task<IActionResult> Identify([FromBody] IdentifyRequest request)
        {
            try 
            {
                // 1. リクエスト自体のチェック
                if (string.IsNullOrEmpty(request.Descriptor)) 
                    return BadRequest("Descriptor is empty");

                float[]? inputDescriptor;
                try {
                    inputDescriptor = JsonSerializer.Deserialize<float[]>(request.Descriptor);
                } catch {
                    return BadRequest("Invalid Descriptor JSON format");
                }

                if (inputDescriptor == null || inputDescriptor.Length == 0) 
                    return BadRequest("Descriptor array is empty");

                // ★修正: int ではなく Guid で受け取る
                Guid currentOrgId = GetCurrentOrganizationId();

                // 2. 自分の組織のFaceMemoを取得 (Where句はGuid同士の比較になります)
                var allFaces = await _context.FaceMemos
                                             .Where(f => f.OrganizationId == currentOrgId)
                                             .Include(f => f.ConversationLogs)
                                             .ToListAsync();
                
                FaceMemo? bestMatch = null;
                double minDistance = 0.6;

                foreach (var face in allFaces)
                {
                    // データ不備への防御コード
                    if (string.IsNullOrEmpty(face.FaceDescriptorJson)) continue;

                    try 
                    {
                        var storedDescriptor = JsonSerializer.Deserialize<float[]>(face.FaceDescriptorJson);
                        
                        // 配列の長さが違う場合は計算できないのでスキップ
                        if (storedDescriptor != null && storedDescriptor.Length == inputDescriptor.Length)
                        {
                            var distance = EuclideanDistance(inputDescriptor, storedDescriptor);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                bestMatch = face;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing face ID {face.Id}: {ex.Message}");
                        continue; 
                    }
                }

                if (bestMatch != null)
                {
                    var logs = await _context.ConversationLogs
                        .Where(l => l.FaceMemoId == bestMatch.Id)
                        .OrderByDescending(l => l.Date)
                        .Take(5)
                        .Select(l => new { l.Date, l.Content })
                        .ToListAsync();

                    return Ok(new { 
                        success = true, 
                        id = bestMatch.Id,
                        name = bestMatch.Name, 
                        affiliation = bestMatch.Affiliation, 
                        notes = bestMatch.Notes,
                        logs = logs 
                    });
                }
                
                return Ok(new { success = false, message = "Unknown" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR in Identify: {ex.Message}");
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }

        // 2. 顔の登録 (POST: api/face/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] FaceMemo model)
        {
            try {
                // 必須項目のチェック
                if(string.IsNullOrEmpty(model.FaceDescriptorJson))
                    return BadRequest("顔データが不足しています");

                // ★修正: ここでGuidのIDが入る
                model.OrganizationId = GetCurrentOrganizationId();
                model.CreatedAt = DateTime.Now;

                _context.FaceMemos.Add(model);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, name = model.Name });
            }
            catch(Exception ex) {
                Console.WriteLine($"Register Error: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        // 3. 会話ログの追加 (POST: api/face/add_log)
        [HttpPost("add_log")]
        public async Task<IActionResult> AddLog([FromBody] LogRequest request)
        {
            try {
                // ★修正: Guidで取得
                Guid currentOrgId = GetCurrentOrganizationId();
                
                // ★修正: OrganizationId (Guid) で検索
                var face = await _context.FaceMemos
                                         .FirstOrDefaultAsync(f => f.Id == request.FaceId && f.OrganizationId == currentOrgId);

                if (face == null) return NotFound("User not found or access denied");

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
            catch(Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }

        private double EuclideanDistance(float[] d1, float[] d2)
        {
            double sum = 0.0;
            for (int i = 0; i < d1.Length; i++) sum += Math.Pow(d1[i] - d2[i], 2);
            return Math.Sqrt(sum);
        }
    }

    public class IdentifyRequest { public string Descriptor { get; set; } = ""; }
    public class LogRequest { public int FaceId { get; set; } public string Content { get; set; } = ""; }
}