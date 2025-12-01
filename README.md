# 2025-employment-1

初期設定に関しては同じ階層のmemo.txtに記載




現在の環境で利用可能なgeminiAPIのモデルの検索方法
実行後、開発者ツールからコンソールを開いてそこに

手入力でallow pastingを入力して貼り付けできるようにして

async function checkAvailableModels() {
    const apiKey = typeof GEMINI_API_KEY !== 'undefined' ? GEMINI_API_KEY : "AIzaSyACdCeSKwHlLhl55na5Jl23ESUbyFaHOnA";
    
    console.log("問い合わせ中...");
    try {
        const response = await fetch(`https://generativelanguage.googleapis.com/v1beta/models?key=${apiKey}`);
        const data = await response.json();
        
        if (data.models) {
            console.log("=== 利用可能なモデル一覧 ===");
            // generateContentメソッドに対応しているモデルのみ抽出
            const generateModels = data.models
                .filter(m => m.supportedGenerationMethods.includes("generateContent"))
                .map(m => m.name.replace("models/", "")); // "models/" を除去して表示
            
            console.table(generateModels);
            alert("利用可能なモデルがコンソールに表示されました。\n一覧の中から選んでコードに記述してください。");
        } else {
            console.error("モデルリストの取得に失敗:", data);
            alert("モデルリストが取得できませんでした。APIキーを確認してください。");
        }
    } catch (e) {
        console.error("通信エラー:", e);
    }
}

checkAvailableModels();

上記コードを実行すると利用できるモデル一覧が返される