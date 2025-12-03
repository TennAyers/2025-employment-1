using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Data;
using _2025_employment_1.Models;

namespace _2025_employment_1.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    // ==========================================
    //  ログイン機能 (UUID対応)
    // ==========================================
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // DBからユーザー検索
        var user = await _context.Users
            .Include(u => u.Organization)
            .FirstOrDefaultAsync(u => u.Email == email);

        // 認証チェック
        if (user == null || 
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
        {
            ViewBag.Error = "メールアドレスまたはパスワードが違います。";
            return View();
        }

        // ★修正点：Guid型のIDを文字列に変換してClaimに保存
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // UUID -> String
            new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
            new Claim("OrganizationId", user.OrganizationId.ToString() ?? "")
        };

        var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

        return RedirectToAction("Index", "Home");
    }

    // ログアウト
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MyCookieAuth");
        return RedirectToAction("Login");
    }

    // ------------------------------------------------
    // 新規登録 (GET)
    // ------------------------------------------------
    [HttpGet]
    public IActionResult Register()
    {
        // ★修正: 組織リストの取得（ドロップダウン用）は廃止します
        // セキュリティのため、リストは渡しません
        return View();
    }

    // ------------------------------------------------
    // 新規登録処理 (POST)
    // ------------------------------------------------
    [HttpPost]
    public async Task<IActionResult> Register(string fullName, string email, string password, string? organizationCode)
    {
        // 1. メール重複チェック
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            ViewBag.Error = "このメールアドレスは既に登録されています。";
            return View();
        }

        // 2. 組織IDの処理 (ここがセキュリティの肝です)
        Guid? finalOrgId = null;

        // 入力がある場合のみチェック
        if (!string.IsNullOrEmpty(organizationCode))
        {
            // A. UUIDの形式として正しいか？
            if (Guid.TryParse(organizationCode, out Guid parsedId))
            {
                // B. そのUUIDを持つ組織が本当にDBに存在するか？
                bool exists = await _context.Organizations.AnyAsync(o => o.Id == parsedId);
                
                if (exists)
                {
                    // 存在すれば採用
                    finalOrgId = parsedId;
                }
                else
                {
                    // UUID形式だけど、DBに登録されていない場合
                    ViewBag.Error = "入力された組織IDが見つかりません。正しいIDを確認してください。";
                    return View();
                }
            }
            else
            {
                // UUIDの形式じゃない場合
                ViewBag.Error = "組織IDの形式が不正です。";
                return View();
            }
        }
        // 入力が空欄の場合は、finalOrgId は null のまま (＝所属なし) となります

        // 3. ユーザー作成
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            OrganizationId = finalOrgId // 検証済みのID または Null
        };
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, password);

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return RedirectToAction("Login");
    }
}