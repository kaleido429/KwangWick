// C# 스크립트와 직접 통신하는 함수들의 라이브러리
mergeInto(LibraryManager.library, {

    InitFFmpeg: async function() {
        try {
            console.log("=== FFmpeg Initialization (No Worker Mode) ===");
            
            if (typeof window.FFmpeg === 'undefined') {
                const script = document.createElement('script');
                script.src = "https://unpkg.com/@ffmpeg/ffmpeg@0.11.6/dist/ffmpeg.min.js";
                document.head.appendChild(script);
                
                await new Promise((resolve, reject) => {
                    script.onload = resolve;
                    script.onerror = reject;
                });
                console.log("✅ FFmpeg script loaded from CDN.");
            }
            
            // [CHANGED] Worker를 완전히 사용하지 않는 방식
            const { createFFmpeg } = window.FFmpeg;
            window.ffmpeg = createFFmpeg({
                log: true,
                corePath: 'https://unpkg.com/@ffmpeg/core@0.11.0/dist/ffmpeg-core.js',
                // [REMOVED] wasmPath와 workerPath 완전 제거 (자동 감지하게 함)
                // [CHANGED] mainThread에서만 실행
                mainThread: true
            });

            console.log("🔄 Loading FFmpeg core...");
            await window.ffmpeg.load();
            
            console.log("✅ FFmpeg loaded successfully (Main Thread Only)!");
            SendMessage('FFmpegController', 'OnFFmpegReady', 'SUCCESS_MAIN_THREAD');

            // === Firebase 초기화 시작 ===
            try {
                if (typeof window.firebase === 'undefined') {
                    console.log("📥 Loading Firebase Compatibility SDKs...");
                    const firebaseScripts = [
                        "https://www.gstatic.com/firebasejs/9.22.2/firebase-app-compat.js",
                        "https://www.gstatic.com/firebasejs/9.22.2/firebase-auth-compat.js",
                        "https://www.gstatic.com/firebasejs/9.22.2/firebase-storage-compat.js"
                    ];

                    for (const src of firebaseScripts) {
                        const script = document.createElement('script');
                        script.src = src;
                        document.head.appendChild(script);
                        await new Promise((resolve, reject) => {
                            script.onload = resolve;
                            script.onerror = reject;
                        });
                    }
                    console.log("✅ Firebase SDKs loaded.");

                    const firebaseConfig = {
                        apiKey: "AIzaSyAGJ38XdodPCcUfpowFODuRx4pKotrbCS0",
                        authDomain: "fairplayfairy-3e2eb.firebaseapp.com",
                        projectId: "fairplayfairy-3e2eb",
                        storageBucket: "fairplayfairy-3e2eb.firebasestorage.app",
                        messagingSenderId: "650162719276",
                        appId: "1:650162719276:web:90442b070eb8a72e385f89",
                        measurementId: "G-GD8XLV1XDG"
                    };

                    if (!window.firebase.apps.length) {
                        window.firebase.initializeApp(firebaseConfig);
                        console.log("✅ Firebase Initialized.");
                    }

                    await window.firebase.auth().signInAnonymously();
                    console.log("✅ Firebase signed in anonymously.");

                } else {
                    console.log("✅ Firebase already initialized.");
                }
            } catch (error) {
                console.error("❌ Firebase initialization failed:", error);
            }
            // === Firebase 초기화 끝 ===

        } catch (error) {
            console.error("❌ FFmpeg initialization failed:", error);
            SendMessage('FFmpegController', 'OnFFmpegFailed', error.message);
        }
    },

    startRecording: function(width, height, framerate) {
        try {
            if (!window.ffmpeg || !window.ffmpeg.isLoaded()) {
                console.error("FFmpeg not ready");
                return;
            }
            
            const canvas = document.getElementById('unity-canvas');
            if (!canvas) return;
            
            console.log(`🎬 Starting recording... (${width}x${height} @ ${framerate}fps)`);
            window.recordingStartTime = new Date();
            
            const stream = canvas.captureStream(framerate);
            window.tempRecorder = new MediaRecorder(stream, { mimeType: 'video/webm;codecs=vp8' });
            window.tempChunks = [];

            window.tempRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) window.tempChunks.push(event.data);
            };
            
            window.tempRecorder.onstop = async () => {
                console.log("⏹️ Recording stopped. Processing...");
                
                try {
                    if (!window.tempChunks || window.tempChunks.length === 0) {
                        throw new Error("No data recorded.");
                    }
                    
                    const totalSize = window.tempChunks.reduce((sum, chunk) => sum + chunk.size, 0);
                    console.log(`📊 Recorded data: ${(totalSize/1024/1024).toFixed(2)} MB`);
                    
                    const inputBlob = new Blob(window.tempChunks, { type: 'video/webm' });
                    const inputData = new Uint8Array(await inputBlob.arrayBuffer());
                    
                    window.ffmpeg.FS('writeFile', 'input.webm', inputData);

                    const startTime = window.recordingStartTime || new Date();
                    const outputFileName = `KwangWick-${startTime.getTime()}.mp4`;
                    
                    console.log(`📝 Converting to ${outputFileName}...`);
                    
                    // [FIXED] 홀수 해상도 문제 해결
                    await window.ffmpeg.run(
                        '-i', 'input.webm',
                        '-c:v', 'libx264',                           // 명시적 코덱 지정
                        '-preset', 'ultrafast',                      // 빠른 처리
                        '-crf', '35',                               // 더 높은 압축률
                        '-vf', 'scale=trunc(iw/2)*2:trunc(ih/2)*2', // 홀수 해상도를 짝수로 변환
                        '-an',                                      // 오디오 제거
                        outputFileName
                    );
                    
                    const outputData = window.ffmpeg.FS('readFile', outputFileName);
                    if (outputData.length === 0) throw new Error("Conversion failed.");
                    
                    const outputBlob = new Blob([outputData.buffer], { type: 'video/mp4' });

                    //firebase에 업로드
                    try {
                        if (window.firebase && window.firebase.storage) {
                            console.log(`☁️ Uploading ${outputFileName} to Firebase Storage...`);
                            const storageRef = window.firebase.storage().ref();
                            const videoRef = storageRef.child('videos/' + outputFileName);
                            
                            const snapshot = await videoRef.put(outputBlob);
                            const downloadURL = await snapshot.ref.getDownloadURL();
                            
                            console.log('✅ Firebase Upload Success! URL:', downloadURL);
                            SendMessage('FFmpegController', 'OnUploadComplete', 'SUCCESS: ' + downloadURL);
                        } else {
                            throw new Error("Firebase Storage is not initialized.");
                        }
                    } catch (error) {
                        console.error("❌ Firebase upload failed:", error);
                        SendMessage('FFmpegController', 'OnUploadComplete', 'FAIL: ' + error.message);

                        // 업로드 실패 시 로컬로 저장 (Fallback)
                        console.log("...업로드 실패. 로컬 다운로드를 시작합니다.");
                        const url = window.URL.createObjectURL(outputBlob);
                        const a = document.createElement('a');
                        a.href = url;
                        a.download = outputFileName;
                        a.style.display = 'none';
                        document.body.appendChild(a);
                        a.click();
                        document.body.removeChild(a);
                        window.URL.revokeObjectURL(url);
                        console.log("🎉 Video saved locally as fallback:", outputFileName);
                    }
            
                    // 인코딩 및 후처리 작업이 완료되었음을 알립니다.
                    SendMessage('FFmpegController', 'OnEncodeComplete', 'SUCCESS: ' + outputFileName);
                    
                    // 메모리 정리
                    window.ffmpeg.FS('unlink', 'input.webm');
                    window.ffmpeg.FS('unlink', outputFileName);
                    
                } catch (error) {
                    console.error("❌ Conversion failed:", error);
                    SendMessage('FFmpegController', 'OnEncodeComplete', 'FAIL: ' + error.message);
                }
            };

            window.tempRecorder.start();

        } catch (error) { 
            console.error("❌ Recording error:", error); 
        }
    },
    
    stopRecording: function() {
        if (window.tempRecorder && window.tempRecorder.state === 'recording') {
            window.tempRecorder.stop();
        }
    }
});