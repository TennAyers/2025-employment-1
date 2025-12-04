using _2025_employment_1.Data;
using Microsoft.EntityFrameworkCore;
using _2025_employment_1.Hubs;

var builder = WebApplication.CreateBuilder(args);

// DB接続
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ★認証設定：ログイン画面の場所を「/Account/Login」に変更しました
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.Cookie.Name = "MyApp.Session";
        options.LoginPath = "/Account/Login"; // ← ここを変更！AccountControllerのLoginアクションへ飛ばします
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); 

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 認証・認可（順番重要）
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<ChatHub>("/chathub"); // ★修正: Hubs.ChatHub ではなく ChatHub でOKです

app.Run();