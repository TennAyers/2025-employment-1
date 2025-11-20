using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _2025_employment_1.Models;
using _2025_employment_1.Data; // これが必要
using Microsoft.EntityFrameworkCore; // これが必要

namespace _2025_employment_1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context; // DBコンテキスト

    // コンストラクタでDBを受け取る
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
    
    

    // 1. 編集画面の表示 (GET)
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        // 保存されている全データを取得してViewに渡す
        // (テーブル名が FaceMemos であると仮定。なければ作成してください)
        var faces = await _context.FaceMemos.ToListAsync();
        return View(faces);
    }

    // 2. 更新処理 (POST)
    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, string affiliation, string notes)
    {
        var face = await _context.FaceMemos.FindAsync(id);
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
        var face = await _context.FaceMemos.FindAsync(id);
        if (face != null)
        {
            _context.FaceMemos.Remove(face);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Edit");
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}