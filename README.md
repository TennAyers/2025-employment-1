# 2025-employment-1

初期設定に関しては同じ階層のmemo.txtに記載




現在の環境で利用可能なgeminiAPIのモデルの検索方法
実行後、開発者ツールからコンソールを開いてそこに

手入力でallow pastingを入力して貼り付けできるようにして

async function checkAvailableModels() {
    const apiKey = typeof GEMINI_API_KEY !== 'undefined' ? GEMINI_API_KEY : "AIzaSyBHit3eVWsJSv_qJLML_xR30xmWiAtZXD8";
    
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



No,組織名,UUID
1,本社開発部,d8591a27-6350-4d4f-9818-701385055051
2,大阪支店,2656f481-2b7e-46bb-b778-433555207212
3,東京営業所,a0f3d4c4-7230-4596-a447-735952331584
4,名古屋センター,e198084e-2868-4503-b0e6-990747424422
5,福岡ラボ,83907106-e752-4a00-9833-286c0716c514
6,札幌サテライト,c3182512-8806-444a-953e-5d15c8172901
7,海外事業部,f5e1b2a9-0d8c-4e6f-924b-3d7a8b5c9e0d
8,品質管理課,9d7f6c3a-1b2e-48a5-bc9d-4e5f6a7b8c9d
9,人事総務部,b2a4c6d8-9e0f-41a3-8c5d-7b9e0f2a4c6d
10,第2開発室,4e6f8a0b-2c4d-6e8f-0a2b-4c6d8e0f2a4b

