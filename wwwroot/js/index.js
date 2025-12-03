
    // =======================================================
    // 0. グローバル変数・設定
    // =======================================================
    
    // 【重要】ここに新しいAPIキーを貼り付けてください
    // C#の変数をJavaScriptの文字列として展開する
    const GEMINI_API_KEY = "@geminiKey";

    // DOM要素
    const video = document.getElementById('video');
    const canvas = document.getElementById('overlay'); // 顔枠用
    const ctx = canvas.getContext('2d');
    const statusEl = document.getElementById('status');
    const detNameEl = document.getElementById('detName');
    const detAffiliationEl = document.getElementById('detAffiliation');
    const logContainer = document.getElementById('logContainer');
    const logInputArea = document.getElementById('logInputArea');
    const newLogInput = document.getElementById('newLogContent');

    // 指カーソル専用キャンバス（最前面）作成
    const cursorCanvas = document.createElement('canvas');
    const cursorCtx = cursorCanvas.getContext('2d');
    cursorCanvas.id = 'cursor-layer';
    Object.assign(cursorCanvas.style, {
        position: 'absolute', top: '50%', left: '50%',
        transform: 'translate(-50%, -50%)', pointerEvents: 'none', zIndex: '1000'
    });
    document.querySelector('.dashboard-container').appendChild(cursorCanvas);

    // 状態管理変数
    let displaySize = { width: 640, height: 480 };
    let lastDetectedDescriptor = null;
    let currentIdentifiedUserId = null;
    let hands = null;
    let lastHandLandmarks = null;
    let activeRecognition = null;

    // パフォーマンス調整用
    let frameCount = 0;
    let lastDetections = [];
    let isProcessingFace = false;

    // 指カーソル用
    let cursorX = 0, cursorY = 0;
    const SMOOTHING_FACTOR = 0.8;
    let isPinching = false, wasPinching = false;

    // ジェスチャー（チェックマーク）用変数
    let gestureCooldown = false;     // 連打防止用
    let lastFocusedInputId = 'newLogContent'; // デフォルトの入力先

    // =======================================================
    // 1. フォーカス追跡 & 音声入力
    // =======================================================
    
    // 入力欄がフォーカスされたら、それを「マイク対象」として記憶する
    window.addEventListener('load', () => {
        const inputs = document.querySelectorAll('input[type="text"], textarea');
        inputs.forEach(input => {
            input.addEventListener('focus', () => {
                lastFocusedInputId = input.id;
            });
            // クリック時もフォーカスとみなす
            input.addEventListener('click', () => {
                lastFocusedInputId = input.id;
            });
        });
    });

    // 音声認識開始・停止関数
    window.startSpeech = function(targetId, btn) {
        // フォーカスを更新
        lastFocusedInputId = targetId;

        if (!('webkitSpeechRecognition' in window)) {
            alert("Chromeブラウザを使用してください。");
            return;
        }

        // 既に起動中なら停止処理
        if (activeRecognition) {
            activeRecognition.stop();
            activeRecognition = null;
            if(btn) btn.classList.remove('listening');
            if (statusEl) statusEl.innerText = "待機中";
            return;
        }

        // 新規開始
        const recognition = new webkitSpeechRecognition();
        recognition.lang = 'ja-JP';
        recognition.interimResults = true;
        const isContinuous = (targetId === 'newLogContent'); // 会話ログのみ連続入力
        recognition.continuous = isContinuous;

        const inputEl = document.getElementById(targetId);
        let baseText = inputEl.value;

        // UI更新
        if(btn) btn.classList.add('listening');
        if (statusEl) {
            statusEl.innerText = isContinuous ? "会話記録中..." : "聞き取り中...";
            statusEl.style.color = isContinuous ? "#ff0055" : "#00d4ff";
        }

        let finalTranscriptBuffer = baseText;
        if (finalTranscriptBuffer && !finalTranscriptBuffer.endsWith(' ')) finalTranscriptBuffer += ' ';

        recognition.onresult = (event) => {
            let interim = '';
            let newFinal = '';
            for (let i = event.resultIndex; i < event.results.length; ++i) {
                if (event.results[i].isFinal) {
                    newFinal += event.results[i][0].transcript;
                } else {
                    interim += event.results[i][0].transcript;
                }
            }
            if (newFinal) {
                finalTranscriptBuffer += newFinal + (isContinuous ? "、" : "");
            }
            inputEl.value = finalTranscriptBuffer + interim;
            inputEl.scrollTop = inputEl.scrollHeight;
        };

        recognition.onend = () => {
            // 自動的に止まった場合の処理
            if (activeRecognition === recognition) {
                activeRecognition = null;
                if(btn) btn.classList.remove('listening');
                if (statusEl) {
                    statusEl.innerText = "待機中";
                    statusEl.style.color = "#aaa";
                }
            }
        };

        activeRecognition = recognition;
        recognition.start();
    };

    // =======================================================
    // 2. モデル読み込み
    // =======================================================
    async function loadModels() {
        try {
            statusEl.innerText = "AIモデル読み込み中...";
            await faceapi.nets.tinyFaceDetector.loadFromUri('/js/models');
            await faceapi.nets.faceLandmark68Net.loadFromUri('/js/models');
            await faceapi.nets.faceRecognitionNet.loadFromUri('/js/models');

            if (typeof Hands === 'undefined') {
                statusEl.innerText = "Handsモデルエラー";
                console.error("MediaPipe Hands script failed to load.");
                return false;
            }
            hands = new Hands({locateFile: (file) => `https://cdn.jsdelivr.net/npm/@@mediapipe/hands/${file}`});
            hands.setOptions({
                maxNumHands: 1,
                modelComplexity: 1,
                minDetectionConfidence: 0.5,
                minTrackingConfidence: 0.5
            });
            hands.onResults(results => {
                lastHandLandmarks = results.multiHandLandmarks && results.multiHandLandmarks.length > 0 
                    ? results.multiHandLandmarks[0] : null;
            });

            console.log("モデル読み込み完了");
            statusEl.innerText = "システム準備完了";
            return true;
        } catch (err) {
            console.error("Model Loading Error:", err);
            statusEl.innerText = "モデル読込エラー";
            return false;
        }
    }

    // =======================================================
    // 3. 画面サイズ管理
    // =======================================================
    async function startVideo() {
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ 
                video: { width: { ideal: 1280 }, height: { ideal: 720 } } 
            });
            video.srcObject = stream;
        } catch (err) {
            console.error(err);
            statusEl.innerText = "カメラ起動エラー";
        }
    }

    function updateCanvasSize() {
        if (!video.videoWidth) return;
        const wRatio = window.innerWidth / window.innerHeight;
        const vRatio = video.videoWidth / video.videoHeight;
        let w, h;
        if (wRatio > vRatio) { h = window.innerHeight; w = h * vRatio; } 
        else { w = window.innerWidth; h = w / vRatio; }

        video.style.width = `${w}px`; video.style.height = `${h}px`;
        canvas.width = w; canvas.height = h;
        canvas.style.width = `${w}px`; canvas.style.height = `${h}px`;
        cursorCanvas.width = w; cursorCanvas.height = h;
        cursorCanvas.style.width = `${w}px`; cursorCanvas.style.height = `${h}px`;
        displaySize = { width: w, height: h };
        faceapi.matchDimensions(canvas, displaySize);
    }
    window.addEventListener('resize', updateCanvasSize);

    // =======================================================
    // 4. 指の幾何学計算 & ジェスチャー検知
    // =======================================================
    function lerp(start, end, factor) { return start + (end - start) * factor; }
    
    // 2点間の距離を計算
    function dist(p1, p2) {
        return Math.hypot(p1.x - p2.x, p1.y - p2.y);
    }

    // チェックマーク(L字)ジェスチャー判定
    function isCheckmarkGesture(landmarks) {
        if (!landmarks) return false;

        const wrist = landmarks[0];
        const thumbTip = landmarks[4];
        const indexTip = landmarks[8];
        const indexMCP = landmarks[5]; 
        
        const handScale = dist(wrist, indexMCP);
        if (handScale === 0) return false;

        const foldedFingers = [
            { tip: 12, pip: 10 },
            { tip: 16, pip: 14 },
            { tip: 20, pip: 18 }
        ];

        let areOthersFolded = true;
        for (let f of foldedFingers) {
            const dTip = dist(landmarks[f.tip], wrist);
            const dPip = dist(landmarks[f.pip], wrist);
            if (dTip > dPip * 1.1) {
                areOthersFolded = false; 
                break;
            }
        }
        if (!areOthersFolded) return false;

        const dIndexTip = dist(indexTip, wrist);
        const dIndexMCP = dist(indexMCP, wrist);
        if (dIndexTip < dIndexMCP * 1.5) return false; 

        const tipDistance = dist(thumbTip, indexTip);
        const isLShape = tipDistance > handScale * 0.8;

        return isLShape;
    }

    // 指カーソル処理
    function handleFingerInteraction(landmarks) {
        cursorCtx.clearRect(0, 0, cursorCanvas.width, cursorCanvas.height);
        if (!landmarks) return;

        const indexTip = landmarks[8];
        const targetX = indexTip.x * cursorCanvas.width;
        const targetY = indexTip.y * cursorCanvas.height;

        if (cursorX === 0 && cursorY === 0) {
            cursorX = targetX; cursorY = targetY;
        } else {
            cursorX = lerp(cursorX, targetX, SMOOTHING_FACTOR);
            cursorY = lerp(cursorY, targetY, SMOOTHING_FACTOR);
        }

        const thumbTip = landmarks[4];
        const pinchDist = dist(indexTip, thumbTip);
        isPinching = (pinchDist < 0.05);

        cursorCtx.beginPath();
        cursorCtx.arc(cursorX, cursorY, isPinching ? 10 : 6, 0, 2 * Math.PI);
        cursorCtx.fillStyle = isPinching ? '#ff0055' : '#00d4ff';
        cursorCtx.shadowBlur = 10; cursorCtx.shadowColor = cursorCtx.fillStyle;
        cursorCtx.fill();
        cursorCtx.shadowBlur = 0;
        cursorCtx.strokeStyle = 'white'; cursorCtx.lineWidth = 2; cursorCtx.stroke();

        const rect = cursorCanvas.getBoundingClientRect();
        const screenX = rect.left + cursorX;
        const screenY = rect.top + cursorY;
        const el = document.elementFromPoint(screenX, screenY);

        document.querySelectorAll('.hovered-by-finger').forEach(e => e.classList.remove('hovered-by-finger'));
        if (el) {
            if (['BUTTON', 'INPUT', 'TEXTAREA', 'A'].includes(el.tagName)) {
                el.classList.add('hovered-by-finger');
                if (isPinching && !wasPinching) {
                    el.click();
                    el.focus();
                }
            }
        }
        wasPinching = isPinching;
    }

    // ジェスチャー視覚フィードバック
    function showGestureFeedback(text) {
        const div = document.createElement('div');
        div.innerHTML = text; 
        Object.assign(div.style, {
            position: 'absolute', left: '50%', top: '40%',
            transform: 'translate(-50%, -50%)', fontSize: '4rem',
            color: '#00ff88', fontWeight: 'bold', textShadow: '0 0 20px #00ff88',
            zIndex: '2000', transition: 'opacity 1s ease-out, top 1s ease-out', pointerEvents: 'none'
        });
        document.body.appendChild(div);

        requestAnimationFrame(() => {
            div.style.opacity = '0';
            div.style.top = '30%';
        });
        setTimeout(() => div.remove(), 1000);
    }

    // =======================================================
    // 5. メインループ
    // =======================================================
    async function detectionLoop() {
        if (!video || video.paused || video.ended) {
            requestAnimationFrame(detectionLoop);
            return;
        }

        // 1. 手の検出
        if (hands) await hands.send({image: video});

        // 2. 顔の検出 (負荷軽減のため4回に1回)
        frameCount++;
        if (frameCount % 4 === 0 && !isProcessingFace) {
            isProcessingFace = true;
            faceapi.detectAllFaces(video, new faceapi.TinyFaceDetectorOptions())
                .withFaceLandmarks()
                .withFaceDescriptors()
                .then(detections => {
                    lastDetections = faceapi.resizeResults(detections, displaySize);
                    if (lastDetections.length === 1) {
                        const detection = lastDetections[0];
                        lastDetectedDescriptor = detection.descriptor;
                        // 顔識別API呼び出し
                        fetch('/api/face/identify', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ Descriptor: JSON.stringify(Array.from(detection.descriptor)) })
                        }).then(r => r.json()).then(res => {
                            if (res.success) {
                                detNameEl.innerText = res.name;
                                detAffiliationEl.innerText = res.affiliation || "なし";
                                updateLogView(res.logs);
                                currentIdentifiedUserId = res.id;
                                logInputArea.style.opacity = "1";
                                logInputArea.style.pointerEvents = "auto";
                            } else {
                                detNameEl.innerText = "未登録の対象";
                                detAffiliationEl.innerText = "---";
                                currentIdentifiedUserId = null;
                                logInputArea.style.opacity = "0.5";
                                logInputArea.style.pointerEvents = "none";
                            }
                        }).catch(() => {});
                    }
                    isProcessingFace = false;
                }).catch(() => { isProcessingFace = false; });
        }

        // 3. 描画 (顔枠)
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        if (lastDetections.length === 1) {
            drawTechBox(lastDetections[0].detection.box);
        }

        // 4. 指カーソル & ジェスチャー処理
        handleFingerInteraction(lastHandLandmarks);

        if (lastHandLandmarks) {
            if (isCheckmarkGesture(lastHandLandmarks)) {
                if (!gestureCooldown) {
                    gestureCooldown = true;
                    setTimeout(() => { gestureCooldown = false; }, 1500);

                    const targetBtn = document.querySelector(`button[onclick*="'${lastFocusedInputId}'"]`);
                    
                    if (targetBtn) {
                        const willStart = !targetBtn.classList.contains('listening');
                        const icon = willStart ? '🎙️ ON' : '🔇 OFF';
                        showGestureFeedback(icon);
                        
                        const isTurningOff = targetBtn.classList.contains('listening');
                        const isLogInput = (lastFocusedInputId === 'newLogContent');

                        targetBtn.click(); // マイク切り替え

                        if (isTurningOff && isLogInput) {
                          showGestureFeedback("🚀 Auto Submit");
                          setTimeout(() => {
                            document.getElementById('addLogButton').click();
                          }, 1000); 
                        }
                    } else {
                        showGestureFeedback("⚠️ No Input Selected");
                    }
                }
            }
        }

        requestAnimationFrame(detectionLoop);
    }

    function drawTechBox(box) {
        const { x, y, width: w, height: h } = box;
        ctx.strokeStyle = '#00d4ff'; ctx.lineWidth = 2;
        ctx.strokeRect(x, y, w, h);
        const lineLen = 20;
        ctx.beginPath();
        ctx.strokeStyle = '#00ff88'; ctx.lineWidth = 4;
        ctx.moveTo(x, y + lineLen); ctx.lineTo(x, y); ctx.lineTo(x + lineLen, y);
        ctx.moveTo(x + w - lineLen, y); ctx.lineTo(x + w, y); ctx.lineTo(x + w, y + lineLen);
        ctx.moveTo(x + w, y + h - lineLen); ctx.lineTo(x + w, y + h); ctx.lineTo(x + w - lineLen, y + h);
        ctx.moveTo(x + lineLen, y + h); ctx.lineTo(x, y + h); ctx.lineTo(x, y + h - lineLen);
        ctx.stroke();
    }

    function updateLogView(logs) {
        if (!logs || logs.length === 0) {
            logContainer.innerHTML = '<div style="text-align:center; color:#666; margin-top:20px;">履歴なし</div>';
            return;
        }
        let html = '';
        logs.forEach(l => {
            const date = new Date(l.date).toLocaleString();
            html += `<div class="log-item"><div class="log-date">${date}</div><div class="log-content">${l.content}</div></div>`;
        });
        logContainer.innerHTML = html;
    }

    // =======================================================
    // 6. Gemini要約 & データ登録
    // =======================================================
    async function summarizeWithGemini(text) {
        // APIキーチェック
        if (!GEMINI_API_KEY || GEMINI_API_KEY === "YOUR_NEW_API_KEY_HERE") {
             alert("ソースコード内の GEMINI_API_KEY に正しいAPIキーを設定してください。");
             return text; 
        }

        // モデル名を修正
        const modelName = "gemini-2.5-flash"; 
        const url = `https://generativelanguage.googleapis.com/v1beta/models/${modelName}:generateContent?key=${GEMINI_API_KEY}`;
        
        const systemPrompt = `
あなたはプロのプロジェクトマネージャーです。以下の会話ログ等のテキストを読み、要約してください。
ただし録音の精度が悪いので文脈から会話内容を適切に補完・修正してください。
また、相手との会話から得られるその人物のプロファイル情報があれば含めてください。

# 制約条件
1. **構成**: 【実施内容】【課題・問題点】【次回アクション】の3見出し。内容がない場合は会話内容をきれいに整えて要約する。
2. **形式**: 箇条書き。
3. **文体**: ビジネスライク。
4. **長さ**: 300文字以内。
5. **重要**: 人名や固有名詞など重要な情報は残すこと。
6. **言語**: 日本語。

# 対象テキスト
${text}`;

        try {
            const response = await fetch(url, {
                method: 'POST', 
                headers: { 'Content-Type': 'application/json' }, 
                body: JSON.stringify({ contents: [{ parts: [{ text: systemPrompt }] }] })
            });
            
            // エラーハンドリングの強化
            if (!response.ok) {
                const errorData = await response.json();
                console.error("🔴 Gemini API Error:", errorData);
                // エラーの詳細をアラートで出す（デバッグ用）
                if (errorData.error && errorData.error.message) {
                    alert("AIエラー: " + errorData.error.message);
                } else {
                    alert("AI通信エラー: " + response.status);
                }
                return text; // エラー時は元のテキストを返す
            }

            const data = await response.json();
            if (data.candidates && data.candidates[0].content) {
                console.log("🟢 Gemini Success:", data);
                return data.candidates[0].content.parts[0].text;
            } else { 
                console.warn("⚠️ Gemini returned no content:", data);
                return text; 
            }
        } catch (error) { 
            console.error("🔴 Network/Script Error:", error); 
            return text; 
        }
    }

    // イベント登録
    startVideo();
    video.addEventListener('loadedmetadata', () => {
        updateCanvasSize();
        video.play();
    });
    video.addEventListener('play', async () => {
        const loaded = await loadModels();
        if (loaded) requestAnimationFrame(detectionLoop);
    });

    document.getElementById('registerButton').addEventListener('click', async () => {
        if (!lastDetectedDescriptor) { alert("顔未検出"); return; }
        const data = {
            Name: document.getElementById('regName').value,
            Affiliation: document.getElementById('regAffiliation').value,
            Notes: document.getElementById('regNotes').value,
            FaceDescriptorJson: JSON.stringify(Array.from(lastDetectedDescriptor)) 
        };
        try {
            const res = await fetch('/api/face/register', {
                method: 'POST', headers: {'Content-Type': 'application/json'},
                body: JSON.stringify(data)
            });
            const result = await res.json();
            if(result.success) {
                alert("登録完了");
                document.getElementById('regName').value = "";
                document.getElementById('regAffiliation').value = "";
                document.getElementById('regNotes').value = "";
            } else { alert("エラー: " + result.message); }
        } catch(e) {}
    });

    document.getElementById('addLogButton').addEventListener('click', async () => {
        if (activeRecognition) { activeRecognition.stop(); }
        const originalContent = newLogInput.value;
        if (!currentIdentifiedUserId || !originalContent) { alert("対象認識または入力不足"); return; }

        const btn = document.getElementById('addLogButton');
        const originalBtnText = btn.innerText;
        btn.innerText = "AI処理中..."; 
        btn.disabled = true;
        
        try {
            // 要約実行
            const summarizedText = await summarizeWithGemini(originalContent);
            
            // サーバーへ送信
            await fetch('/api/face/add_log', {
                method: 'POST', headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({ FaceId: currentIdentifiedUserId, Content: summarizedText })
            });
            
            // 画面更新
            const date = new Date().toLocaleString();
            logContainer.insertAdjacentHTML('afterbegin', 
                `<div class="log-item" style="border-left:3px solid #ff0055;padding-left:5px;"><div class="log-date">${date}</div><div class="log-content">${summarizedText}</div></div>`);
            newLogInput.value = "";
        } catch(e) { 
            console.error(e); 
            alert("保存処理に失敗しました");
        } 
        finally { 
            btn.innerText = originalBtnText; 
            btn.disabled = false; 
        }
    });