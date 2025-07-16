// C# 스크립트와 직접 통신하는 함수들의 라이브러리
mergeInto(LibraryManager.library, {

    // [NEW] 변환된 비디오 데이터를 임시 저장할 전역 변수
    $convertedVideoBlob: null,

    InitFFmpeg: async function(FirebaseConfig) {
        const jsonConfig = UTF8ToString(FirebaseConfig);
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
            
            const { createFFmpeg } = window.FFmpeg;
            window.ffmpeg = createFFmpeg({
                log: false,
                corePath: 'https://unpkg.com/@ffmpeg/core@0.11.0/dist/ffmpeg-core.js',
                mainThread: false
            });

            console.log("🔄 Loading FFmpeg core...");
            await window.ffmpeg.load();
            
            console.log("✅ FFmpeg loaded successfully");
            

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
                    const firebaseConfig = JSON.parse(jsonConfig);
                    
                    if (!window.firebase.apps.length) {
                        window.firebase.initializeApp(firebaseConfig);
                        console.log("✅ Firebase Initialized.");
                    }

                    await window.firebase.auth().signInAnonymously();
                    console.log("✅ Firebase signed in anonymously.");
                    const user = window.firebase.auth().currentUser;
                    console.log("인증 상태:", user ? "인증됨" : "인증 안 됨");
                    SendMessage('FFmpegController', 'OnFFmpegReady', 'SUCCESS_MAIN_THREAD');
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
            
            const stream = canvas.captureStream(framerate);
            window.tempRecorder = new MediaRecorder(stream, { mimeType: 'video/webm;codecs=vp8' });
            window.tempChunks = [];

            window.tempRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) window.tempChunks.push(event.data);
            };
            
            // [REFACTORED] onstop 이벤트 핸들러: 변환 후 임시 저장까지만 수행
            window.tempRecorder.onstop = async () => {
                console.log("⏹️ Recording stopped. Processing...");
                const startEncodingTime = performance.now();
                try {
                    if (!window.tempChunks || window.tempChunks.length === 0) {
                        throw new Error("No data recorded.");
                    }
                    
                    const inputBlob = new Blob(window.tempChunks, { type: 'video/webm' });
                    const inputData = new Uint8Array(await inputBlob.arrayBuffer());
                    
                    window.ffmpeg.FS('writeFile', 'input.webm', inputData);
                    
                    console.log(`📝 Converting to MP4...`);
                    
                    await window.ffmpeg.run(
                        '-i', 'input.webm',
                        '-c:v', 'libx264',
                        '-preset', 'ultrafast',
                        '-crf', '35',
                        '-vf', 'scale=trunc(iw/2)*2:trunc(ih/2)*2',
                        '-an',
                        'output.mp4' // 임시 파일명 사용
                    );
                    
                    const outputData = window.ffmpeg.FS('readFile', 'output.mp4');
                    if (outputData.length === 0) throw new Error("Conversion failed.");
                    
                    // [CHANGED] 변환된 비디오 데이터를 전역 변수에 저장
                    window.convertedVideoBlob = new Blob([outputData.buffer], { type: 'video/mp4' });
                    const endEncodingTime = performance.now();
                    const encodingTime = (endEncodingTime - startEncodingTime) / 1000;
                    console.log(`✅ Video converted and ready for upload. (Encoding time: ${encodingTime}s)`);

                    // C#으로 변환 완료를 알림
                    SendMessage('FFmpegController', 'OnEncodeComplete', 'SUCCESS');
                    
                    // 메모리 정리
                    window.ffmpeg.FS('unlink', 'input.webm');
                    window.ffmpeg.FS('unlink', 'output.mp4');
                    
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
    },

    // [NEW] C#에서 호출하는 업로드 전용 함수
    uploadVideo: async function(filenamePtr) {
        const startUploadTime = performance.now();
        try {
            const filename = UTF8ToString(filenamePtr) + ".mp4";
            const videoBlob = window.convertedVideoBlob;

            if (!videoBlob) {
                throw new Error("No converted video data found to upload.");
            }
            if (!window.firebase || !window.firebase.storage) {
                throw new Error("Firebase Storage is not initialized.");
            }

            console.log(`☁️ Uploading ${filename} to Firebase Storage...`);
            const storageRef = window.firebase.storage().ref();
            const videoRef = storageRef.child('videos/' + filename);
            
            const snapshot = await videoRef.put(videoBlob);
            const downloadURL = await snapshot.ref.getDownloadURL();
            
            const endUploadTime = performance.now();
            const uploadTime = (endUploadTime - startUploadTime) / 1000;
            console.log(`✅ Firebase Upload Success! URL: ${downloadURL} (Upload time: ${uploadTime}s)`);
        } catch (error) {
            console.error("❌ Firebase upload failed:", error);
        } finally {
            // 업로드 성공/실패와 관계없이 임시 데이터 정리
            window.convertedVideoBlob = null;
        }
    }
});