mergeInto(LibraryManager.library, {
    TestFFmpegLoad: function() {
        console.log("=== FFmpeg v0.11.6 Load Test Started (No Worker) ===");
        
        if (typeof window.FFmpeg === 'undefined') {
            console.log("❌ window.FFmpeg is undefined");
            SendMessage('FFmpegController', 'OnFFmpegLoadTest', 'FAIL: window.FFmpeg undefined');
            return;
        }
        
        console.log("✅ window.FFmpeg exists");
        console.log("FFmpeg object:", window.FFmpeg);
        console.log("FFmpeg keys:", Object.keys(window.FFmpeg));
        
        // v0.11.6의 createFFmpeg 함수 확인
        if (typeof window.FFmpeg.createFFmpeg === 'undefined') {
            console.log("❌ window.FFmpeg.createFFmpeg is undefined");
            SendMessage('FFmpegController', 'OnFFmpegLoadTest', 'FAIL: createFFmpeg undefined');
            return;
        }
        
        console.log("✅ All FFmpeg v0.11.6 functions available");
        SendMessage('FFmpegController', 'OnFFmpegLoadTest', 'SUCCESS: FFmpeg v0.11.6 loaded and ready');
    },
    
    InitFFmpegTest: async function() {
        console.log("=== FFmpeg Initialization Test Started (v0.11.6 No Worker Mode) ===");
        
        if (!window.FFmpeg || !window.FFmpeg.createFFmpeg) {
            console.log("❌ FFmpeg not available for initialization");
            SendMessage('FFmpegController', 'OnFFmpegInitTest', 'FAIL: FFmpeg not available');
            return;
        }
        
        try {
            console.log("Creating FFmpeg instance (v0.11.6 - Worker disabled)...");
            
            // v0.11.6의 createFFmpeg API 사용 (Worker 비활성화)
            const { createFFmpeg } = window.FFmpeg;
            
            window.ffmpeg = createFFmpeg({
                log: true,
                corePath: 'StreamingAssets/ffmpeg-core.js',     // 로컬 파일 사용
                wasmPath: 'StreamingAssets/ffmpeg-core.wasm',   // 로컬 파일 사용
                workerPath: null, // Worker 명시적 비활성화
                multithread: false // 멀티스레드 비활성화
            });
            
            console.log("Loading FFmpeg core (v0.11.0 from unpkg)...");
            
            // 타임아웃 설정
            const loadPromise = window.ffmpeg.load();
            
            const timeoutPromise = new Promise((_, reject) => {
                setTimeout(() => {
                    reject(new Error('Load timeout after 30 seconds'));
                }, 30000);
            });
            
            await Promise.race([loadPromise, timeoutPromise]);
            
            console.log("✅ FFmpeg loaded successfully (single-threaded, no-worker)!");
            console.log("FFmpeg isLoaded:", window.ffmpeg.isLoaded());
            
            // 게임 시작 시간 설정
            window.gameStartTime = new Date();
            console.log("Game start time set:", window.gameStartTime.toISOString());
            
            SendMessage('FFmpegController', 'OnFFmpegInitTest', 'SUCCESS: FFmpeg v0.11.6 initialized (no-worker)');
            
        } catch (error) {
            console.log("❌ FFmpeg initialization failed");
            console.error("Error details:", error);
            console.error("Error stack:", error.stack);
            
            // 특정 오류 유형 분석
            if (error.message && error.message.includes('timeout')) {
                console.log("Timeout error - falling back to MediaRecorder");
                SendMessage('FFmpegController', 'OnFFmpegInitTest', 'TIMEOUT: Falling back to MediaRecorder');
            } else if (error.message && (error.message.includes('worker') || error.message.includes('Worker'))) {
                console.log("Worker error still detected - using MediaRecorder fallback");
                SendMessage('FFmpegController', 'OnFFmpegInitTest', 'WORKER_ERROR: Using MediaRecorder fallback');
            } else if (error.message && error.message.includes('WebAssembly')) {
                console.log("WebAssembly error - trying MediaRecorder fallback");
                SendMessage('FFmpegController', 'OnFFmpegInitTest', 'WASM_ERROR: Using MediaRecorder fallback');
            } else {
                SendMessage('FFmpegController', 'OnFFmpegInitTest', 'FAIL: ' + error.toString());
            }
        }
    },
    
    // MediaRecorder로 폴백하는 함수들
    StartMediaRecording: function() {
        try {
            console.log("📹 [MediaRecorder Mode] Starting MediaRecorder as FFmpeg fallback...");
            
            const canvas = document.getElementById('unity-canvas');
            if (!canvas) {
                console.error("❌ [MediaRecorder Mode] Unity canvas not found");
                return;
            }
            
            // 녹화 시작 시간 기록
            window.recordingStartTime = new Date();
            console.log("📅 [MediaRecorder Mode] Start time:", window.recordingStartTime.toISOString());
            
            const stream = canvas.captureStream(30);
            
            // 지원되는 MIME 타입 찾기
            let mimeType = 'video/webm';
            if (!MediaRecorder.isTypeSupported(mimeType)) {
                mimeType = 'video/webm;codecs=vp8';
                if (!MediaRecorder.isTypeSupported(mimeType)) {
                    mimeType = '';
                }
            }
            
            console.log("🎥 [MediaRecorder Mode] Using MIME type:", mimeType || 'default');
            
            window.mediaRecorder = new MediaRecorder(stream, mimeType ? { mimeType } : {});
            window.recordedChunks = [];
            
            window.mediaRecorder.ondataavailable = function(event) {
                if (event.data.size > 0) {
                    window.recordedChunks.push(event.data);
                    console.log("📊 [MediaRecorder Mode] Data chunk received:", event.data.size, "bytes");
                }
            };
            
            window.mediaRecorder.onstop = function() {
                console.log("⏹️ [MediaRecorder Mode] Recording stopped, processing...");
                ProcessMediaRecording();
            };
            
            window.mediaRecorder.start();
            console.log("✅ [MediaRecorder Mode] MediaRecorder started successfully");
            
        } catch (error) {
            console.error("❌ [MediaRecorder Mode] Error starting MediaRecorder:", error);
        }
    },
    
    StopMediaRecording: function() {
        if (window.mediaRecorder && window.mediaRecorder.state === 'recording') {
            window.mediaRecorder.stop();
        }
    },
    
    // FFmpeg를 사용한 실제 녹화 기능
    StartFFmpegRecording: function() {
        try {
            console.log("🎬 [FFmpeg Mode] Starting FFmpeg recording...");
            
            if (!window.ffmpeg || !window.ffmpeg.isLoaded()) {
                console.error("❌ [FFmpeg Mode] FFmpeg not loaded or ready");
                return;
            }
            
            const canvas = document.getElementById('unity-canvas');
            if (!canvas) {
                console.error("❌ [FFmpeg Mode] Unity canvas not found");
                return;
            }
            
            // 녹화 시작 시간 기록
            window.recordingStartTime = new Date();
            console.log("📅 [FFmpeg Mode] Recording start time:", window.recordingStartTime.toISOString());
            
            // Canvas에서 비디오 스트림 캡처
            const stream = canvas.captureStream(30); // 30 FPS
            
            // MediaRecorder로 임시 녹화 (FFmpeg 처리를 위해)
            window.tempRecorder = new MediaRecorder(stream, { 
                mimeType: 'video/webm;codecs=vp8' 
            });
            
            window.tempChunks = [];
            
            window.tempRecorder.ondataavailable = function(event) {
                if (event.data.size > 0) {
                    window.tempChunks.push(event.data);
                    console.log("📊 [FFmpeg Mode] Data chunk received:", event.data.size, "bytes");
                }
            };
            
            window.tempRecorder.onstop = async function() {
                console.log("⏹️ [FFmpeg Mode] Temp recording stopped, processing with FFmpeg...");
                
                try {
                    if (!window.tempChunks || window.tempChunks.length === 0) {
                        console.error("❌ [FFmpeg Mode] No recorded data");
                        return;
                    }
                    
                    console.log("🔄 [FFmpeg Mode] Converting video to MP4 with FFmpeg...");
                    
                    const inputBlob = new Blob(window.tempChunks, { type: 'video/webm' });
                    const inputData = new Uint8Array(await inputBlob.arrayBuffer());
                    const inputFileName = 'input.webm';
                    window.ffmpeg.FS('writeFile', inputFileName, inputData);

                    const startTime = window.recordingStartTime || new Date();
                    const year = startTime.getFullYear();
                    const month = String(startTime.getMonth() + 1).padStart(2, '0');
                    const day = String(startTime.getDate()).padStart(2, '0');
                    const hour = String(startTime.getHours()).padStart(2, '0');
                    const minute = String(startTime.getMinutes()).padStart(2, '0');
                    const second = String(startTime.getSeconds()).padStart(2, '0');
                    const outputFileName = `${year}-${month}-${day}-${hour}-${minute}-${second}.mp4`;

                    console.log(`📝 [FFmpeg Mode] Starting conversion from ${inputFileName} to ${outputFileName}`);
                    
                    // 홀수 해상도 문제를 해결하고 오디오를 제거하기 위한 옵션 추가
                    await window.ffmpeg.run(
                        '-threads', '1',
                        '-i', inputFileName,
                        '-an', // 오디오 스트림 제거
                        '-vf', 'scale=trunc(iw/2)*2:trunc(ih/2)*2', // 해상도를 짝수로 맞춤
                        '-pix_fmt', 'yuv420p', // 호환성을 위한 픽셀 포맷 설정
                        outputFileName
                    );
                    
                    const outputData = window.ffmpeg.FS('readFile', outputFileName);

                    // 변환 결과 파일이 비어있는지 확인하여 실패 시 에러 발생
                    if (outputData.length === 0) {
                        throw new Error("FFmpeg conversion failed, output file is empty.");
                    }
                    
                    // Create download link for the MP4 file
                    const outputBlob = new Blob([outputData.buffer], { type: 'video/mp4' });
                    const url = window.URL.createObjectURL(outputBlob);
                    const a = document.createElement('a');
                    document.body.appendChild(a);
                    a.style = 'display: none';
                    a.href = url;
                    a.download = outputFileName;
                    a.click();
                    window.URL.revokeObjectURL(url);
                    document.body.removeChild(a);
                    
                    console.log("🎉 [FFmpeg Mode] Video converted and saved successfully:", outputFileName);
                    SendMessage('FFmpegController', 'OnEncodeComplete', '[FFmpeg-MP4] ' + outputFileName);

                    // Clean up files from FFmpeg's virtual file system
                    window.ffmpeg.FS('unlink', inputFileName);
                    window.ffmpeg.FS('unlink', outputFileName);
                    
                } catch (error) {
                    console.error("❌ [FFmpeg Mode] FFmpeg processing failed:", error);
                    
                    // Fallback to downloading the original WebM file
                    console.log("⚠️ [FFmpeg Mode] Falling back to original WebM download...");
                    const startTime = window.recordingStartTime || new Date();
                    const year = startTime.getFullYear();
                    const month = String(startTime.getMonth() + 1).padStart(2, '0');
                    const day = String(startTime.getDate()).padStart(2, '0');
                    const hour = String(startTime.getHours()).padStart(2, '0');
                    const minute = String(startTime.getMinutes()).padStart(2, '0');
                    const second = String(startTime.getSeconds()).padStart(2, '0');
                    
                    const fallbackFileName = `${year}-${month}-${day}-${hour}-${minute}-${second}.webm`;
                    
                    const fallbackBlob = new Blob(window.tempChunks, { type: 'video/webm' });
                    const fallbackUrl = window.URL.createObjectURL(fallbackBlob);
                    const a = document.createElement('a');
                    document.body.appendChild(a);
                    a.style = 'display: none';
                    a.href = fallbackUrl;
                    a.download = fallbackFileName;
                    a.click();
                    window.URL.revokeObjectURL(fallbackUrl);
                    document.body.removeChild(a);
                    
                    SendMessage('FFmpegController', 'OnEncodeComplete', '[FFmpeg Fallback] ' + fallbackFileName);
                }
            };
            
            window.tempRecorder.start();
            console.log("✅ [FFmpeg Mode] FFmpeg recording started successfully");
            
        } catch (error) {
            console.error("❌ [FFmpeg Mode] Error starting FFmpeg recording:", error);
        }
    },
    
    StopFFmpegRecording: function() {
        if (window.tempRecorder && window.tempRecorder.state === 'recording') {
            window.tempRecorder.stop();
        }
    },
    
    // C# bool IsFFmpegReady()'와 연결됩니다.
    // window.isFFmpegReady는 index.html에서 설정하는 전역 변수입니다.
    IsFFmpegReady: function() {
        // C#의 bool은 0 또는 1로 마샬링됩니다.
        return window.isFFmpegReady ? 1 : 0;
    },

    // C# void startRecording()'와 연결됩니다.
    // window.startRecording은 index.html에 정의된 전역 함수입니다.
    startRecording: function() {
        if (typeof window.startRecording === 'function') {
            window.startRecording();
        } else {
            // 함수가 아직 정의되지 않았을 경우를 대비한 오류 처리
            console.error("JS_Bridge_Error: startRecording() function is not defined on the window object. Check index.html.");
        }
    },

    // C# void stopRecording()'와 연결됩니다.
    // window.stopRecording은 index.html에 정의된 전역 함수입니다.
    stopRecording: function() {
        if (typeof window.stopRecording === 'function') {
            window.stopRecording();
        } else {
            // 함수가 아직 정의되지 않았을 경우를 대비한 오류 처리
            console.error("JS_Bridge_Error: stopRecording() function is not defined on the window object. Check index.html.");
        }
    }
});

// MediaRecorder 처리 함수
function ProcessMediaRecording() {
    if (!window.recordedChunks || window.recordedChunks.length === 0) {
        console.error("❌ [MediaRecorder Mode] No recorded data");
        return;
    }
    
    console.log("🔄 [MediaRecorder Mode] Processing recorded data...");
    console.log("📦 [MediaRecorder Mode] Total chunks:", window.recordedChunks.length);
    
    // 녹화 시작 시간 기준 파일명 생성
    const startTime = window.recordingStartTime || new Date();
    const year = startTime.getFullYear();
    const month = String(startTime.getMonth() + 1).padStart(2, '0');
    const day = String(startTime.getDate()).padStart(2, '0');
    const hour = String(startTime.getHours()).padStart(2, '0');
    const minute = String(startTime.getMinutes()).padStart(2, '0');
    const second = String(startTime.getSeconds()).padStart(2, '0');
    
    const fileName = `${year}-${month}-${day}-${hour}-${minute}-${second}.webm`;
    
    // 다운로드
    const blob = new Blob(window.recordedChunks, { type: 'video/webm' });
    console.log("📁 [MediaRecorder Mode] Final blob size:", blob.size, "bytes");
    console.log("📝 [MediaRecorder Mode] Filename:", fileName);
    
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.style = 'display: none';
    a.href = url;
    a.download = fileName;
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
    
    console.log("🎉 [MediaRecorder Mode] Video downloaded:", fileName);
    SendMessage('FFmpegController', 'OnEncodeComplete', '[MediaRecorder] ' + fileName);
}