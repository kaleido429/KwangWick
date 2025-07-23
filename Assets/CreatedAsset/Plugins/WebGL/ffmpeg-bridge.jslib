// C# 스크립트와 직접 통신하는 함수들의 라이브러리
mergeInto(LibraryManager.library, {

    // [MODIFIED] 전역 변수 이름을 recordedVideoBlob으로 통일합니다.
    $recordedVideoBlob: null,

    InitFFmpeg: async function(FirebaseConfig) {
        const jsonConfig = UTF8ToString(FirebaseConfig);
        try {
            /*
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
            */

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
            /*
            if (!window.ffmpeg || !window.ffmpeg.isLoaded()) {
                console.error("FFmpeg not ready");
                return;
            }
            */
            const canvas = document.getElementById('unity-canvas');
            if (!canvas) return;
            
            console.log(`🎬 Starting recording... (${width}x${height} @ ${framerate}fps)`);
            
            const stream = canvas.captureStream(framerate);
            window.tempRecorder = new MediaRecorder(stream, { mimeType: 'video/webm;codecs=vp8' });
            window.tempChunks = [];

            window.tempRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) window.tempChunks.push(event.data);
            };
            
            //녹화 데이터 전역변수에 저장
            // [REFACTORED] onstop 이벤트 핸들러: WebM Blob 생성만 수행
            window.tempRecorder.onstop = async () => {
                console.log("⏹️ Recording stopped. Processing...");
                try {
                    if (!window.tempChunks || window.tempChunks.length === 0) {
                        throw new Error("No data recorded.");
                    }
                    
                    // 녹화된 데이터를 전역 변수에 저장
                    window.recordedVideoBlob = new Blob(window.tempChunks, { type: 'video/webm' });
                    window.tempChunks = []; // 임시 데이터 정리

                    console.log(`✅ WebM video recorded and ready for upload.`);
                    
                    // C#으로 녹화 완료(업로드 준비 완료)를 알림
                    SendMessage('FFmpegController', 'OnEncodeComplete', 'SUCCESS');
                    
                } catch (error) {
                    console.error("❌ Recording processing failed:", error);
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

    // [MODIFIED] C#에서 호출하는 업로드 전용 함수
    uploadVideo: async function(filenamePtr) {
        const startUploadTime = performance.now();
        try {
            const filename = UTF8ToString(filenamePtr) + ".webm";
            // [MODIFIED] recordedVideoBlob에서 데이터를 가져오도록 수정합니다.
            const videoBlob = window.recordedVideoBlob; 

            if (!videoBlob) {
                throw new Error("No recorded video data found to upload.");
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
            // C#으로 업로드 완료 메시지 전송
            SendMessage('FFmpegController', 'UploadComplete', 'SUCCESS');
        } catch (error) {
            console.error("❌ Firebase upload failed:", error);
            SendMessage('FFmpegController', 'UploadComplete', 'FAIL: ' + error.message);
        } finally {
            // [MODIFIED] 업로드 후 정리할 변수 이름도 통일합니다.
            window.recordedVideoBlob = null;
        }
    }
});