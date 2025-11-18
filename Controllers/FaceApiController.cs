using Microsoft.AspNetCore.Mvc;
using _2025_employment_1.Data;
using _2025_employment_1.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace _2025_employment_1.Controllers
{
    [ApiController]
    [Route("api/face")]
    public class FaceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FaceApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ▼▼▼ 1. 顔登録API (変更なし) ▼▼▼
        [HttpPost("register")]
        public async Task<IActionResult> RegisterFace([FromBody] FaceMemo newMemo)
        {
            try
            {
                await _context.FaceMemos.AddAsync(newMemo);
                await _context.SaveChangesAsync(); 
                
                return Ok(new { success = true, id = newMemo.Id, name = newMemo.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ▼▼▼ 2. 顔識別API (修正あり) ▼▼▼
        [HttpPost("identify")]
        public async Task<IActionResult> IdentifyFace([FromBody] IdentifyRequest request)
        {
            var registeredFaces = await _context.FaceMemos.ToListAsync();

            var queryDescriptor = JsonSerializer.Deserialize<float[]>(request.Descriptor);

            // ▼▼▼ 修正点 1 (CS8600 / CS8604) ▼▼▼
            // JSから送られたデータが null ではないかチェックする
            if (queryDescriptor == null)
            {
                return BadRequest(new { success = false, message = "Invalid descriptor data."});
            }

            FaceMemo? bestMatch = null; // <-- null許容に変更
            double bestDistance = 0.6; 

            foreach (var face in registeredFaces)
            {
                var registeredDescriptor = JsonSerializer.Deserialize<float[]>(face.FaceDescriptorJson);

                // ▼▼▼ 修正点 2 (CS8604) ▼▼▼
                // DBから取得したデータも null ではないかチェックする
                if (registeredDescriptor != null)
                {
                    // 類似度（ユークリッド距離）を計算
                    double distance = CalculateEuclideanDistance(queryDescriptor, registeredDescriptor);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestMatch = face;
                    }
                }
            }

            if (bestMatch != null)
            {
                return Ok(new { 
                    success = true, 
                    name = bestMatch.Name, 
                    affiliation = bestMatch.Affiliation, 
                    notes = bestMatch.Notes 
                });
            }
            else
            {
                return Ok(new { success = false, name = "???" });
            }
        }

        // C#で類似度（距離）を計算する関数 (変更なし)
        private double CalculateEuclideanDistance(float[] v1, float[] v2)
        {
            double sum = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                sum += Math.Pow(v1[i] - v2[i], 2);
            }
            return Math.Sqrt(sum);
        }
    }

    // IdentifyAPI が受け取るJSONの型定義
    public class IdentifyRequest
    {
        public string Descriptor { get; set; } = null!; // <-- = null!; を追加
    }
}