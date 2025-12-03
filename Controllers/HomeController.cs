using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _2025_employment_1.Models;
using _2025_employment_1.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // ユーザー情報取得用
using Microsoft.AspNetCore.Authorization; // 認証用

namespace _2025_employment_1.Controllers;

[Authorize] // ログインしていないユーザーはアクセス不可
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // ----------------------------------------------------
    // ホーム画面 (Index)
    // ----------------------------------------------------
    public async Task<IActionResult> Index()
    {
        // ログイン中のユーザーIDを表示用に取得
        ViewBag.currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ViewBag.CurrentUserName = User.Identity?.Name;

        // 組織情報の取得
        Guid? currentOrgId = GetCurrentOrganizationId();

        if (currentOrgId.HasValue)
        {
            // 組織に所属している場合、DBから組織名を取得
            var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == currentOrgId.Value);
            ViewBag.CurrentOrgName = org?.Name ?? "組織名不明";
            ViewBag.CurrentOrgId = currentOrgId.Value.ToString();
        }
        else
        {
            // 個人利用の場合
            ViewBag.CurrentOrgName = "個人利用 (所属なし)";
            ViewBag.CurrentOrgId = "なし";
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    // ----------------------------------------------------
    // ヘルパーメソッド: ログイン中のユーザーの組織IDを取得
    // ----------------------------------------------------
    private Guid? GetCurrentOrganizationId()
    {
        // Claimから "OrganizationId" を取得
        var orgIdString = User.FindFirst("OrganizationId")?.Value;

        // 文字列を Guid に変換。失敗した場合や空の場合は null を返す
        if (Guid.TryParse(orgIdString, out Guid orgId))
        {
            return orgId;
        }

        return null; // 所属なし
    }

    // ----------------------------------------------------
    // 1. 編集・一覧画面の表示 (GET)
    // ----------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        // ユーザー情報の表示用
        ViewBag.currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ViewBag.CurrentUserName = User.Identity?.Name;

        // 1. 自分の組織ID (UUID または null) を取得
        Guid? currentOrgId = GetCurrentOrganizationId();

        // リスト初期化
        List<FaceMemo> faces = new List<FaceMemo>();

        if (currentOrgId.HasValue)
        {
            // --- A. 組織に所属している場合 ---

            // 組織名を取得
            var org = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == currentOrgId.Value);
            ViewBag.CurrentOrgName = org?.Name;
            ViewBag.CurrentOrgId = currentOrgId.Value.ToString();

            // ★セキュリティ: 自分の組織のデータだけを取得
            faces = await _context.FaceMemos
                                  .Where(m => m.OrganizationId == currentOrgId.Value)
                                  .Include(m => m.ConversationLogs) 
                                  .ToListAsync();
        }
        else
        {
            // --- B. 個人利用 (所属なし) の場合 ---
            ViewBag.CurrentOrgName = "個人利用 (データ共有なし)";
            ViewBag.CurrentOrgId = "なし";

            // ※現状のFaceMemoテーブル設計ではOrganizationIdが必須(Guid)の場合、
            // 個人ユーザー用のデータを持てない可能性があります。
            // 暫定的に「何も表示しない」か、個人用データの持ち方に合わせて修正してください。
            faces = new List<FaceMemo>(); 
        }

        return View(faces);
    }

    // ----------------------------------------------------
    // 2. 更新処理 (POST)
    // ----------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, string affiliation, string notes)
    {
        // 自分の組織IDを取得
        Guid? currentOrgId = GetCurrentOrganizationId();

        if (currentOrgId.HasValue)
        {
            // ★セキュリティ重要: 
            // 「指定されたID」かつ「自分の組織IDと一致するもの」だけを検索して更新する
            // これにより、他組織のデータを勝手に書き換えられないようにする
            var face = await _context.FaceMemos
                                     .FirstOrDefaultAsync(m => m.Id == id && m.OrganizationId == currentOrgId.Value);

            if (face != null)
            {
                face.Name = name;
                face.Affiliation = affiliation;
                face.Notes = notes;
                await _context.SaveChangesAsync();
            }
        }
        
        // 個人ユーザー(currentOrgId == null)の場合は更新権限なしとして何もしない

        return RedirectToAction("Edit");
    }

    // ----------------------------------------------------
    // 3. 削除処理 (POST)
    // ----------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        // 自分の組織IDを取得
        Guid? currentOrgId = GetCurrentOrganizationId();

        if (currentOrgId.HasValue)
        {
            // ★セキュリティ重要:
            // 更新と同様に、自分の組織のデータかどうかをチェックしてから削除する
            var face = await _context.FaceMemos
                                     .FirstOrDefaultAsync(m => m.Id == id && m.OrganizationId == currentOrgId.Value);

            if (face != null)
            {
                _context.FaceMemos.Remove(face);
                await _context.SaveChangesAsync();
            }
        }

        return RedirectToAction("Edit");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}