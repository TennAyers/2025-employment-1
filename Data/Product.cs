namespace _2025_employment_1.Models;

public class Product
{
    // 主キー (Primary Key) になります。EF Coreが自動でIDと認識します。
    public int Id { get; set; }

    // 商品名を保存するカラム (データベースではTEXT型になります)
    public string Name { get; set; }

    // 価格を保存するカラム (データベースではINTEGER型になります)
    public int Price { get; set; }

    // 登録日を保存するカラム (データベースではTEXT型になります)
    public DateTime CreatedAt { get; set; }
}