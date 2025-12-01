using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _2025_employment_1.Models;
using _2025_employment_1.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // 追加: ユーザー情報取得用

namespace _2025_employment_1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    // --- ヘルパーメソッド: ログイン中のユーザーの組織IDを取得 ---
    // ※実際にはASP.NET Core Identityなどの認証情報から取得します
    private int GetCurrentOrganizationId()
    {
        // 【重要】認証機能が未実装の場合は、テスト用に「1」などを返してください
        // 実装後は User.FindFirstValue(...) などを使います
        // 例: return int.Parse(User.FindFirst("OrganizationId").Value);
        return 1; // 仮置き: 組織ID 1 として動作
    }

    // 1. 編集画面の表示 (GET)
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        int currentOrgId = GetCurrentOrganizationId();

        // ★修正点: 自分の組織 (OrganizationId) のデータだけを取得するようにフィルタリング
        var faces = await _context.FaceMemos
                                  .Where(m => m.OrganizationId == currentOrgId) // フィルタ追加
                                  .Include(m => m.ConversationLogs) 
                                  .ToListAsync();
                                  
        return View(faces);
    }

    // 2. 更新処理 (POST)
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, string affiliation, string notes)
    {
        int currentOrgId = GetCurrentOrganizationId();

        // ★修正点: IDだけでなく、自分の組織のデータかどうかもチェックして取得
        var face = await _context.FaceMemos
                                 .FirstOrDefaultAsync(m => m.Id == id && m.OrganizationId == currentOrgId);

        if (face != null)
        {
            face.Name = name;
            face.Affiliation = affiliation;
            face.Notes = notes;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Edit");
    }

    // 3. 削除処理 (POST)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        int currentOrgId = GetCurrentOrganizationId();

        // ★修正点: 他の組織のデータを消せないようにチェック
        var face = await _context.FaceMemos
                                 .FirstOrDefaultAsync(m => m.Id == id && m.OrganizationId == currentOrgId);

        if (face != null)
        {
            _context.FaceMemos.Remove(face);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Edit");
    }
}