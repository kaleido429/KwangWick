// C# 스크립트와 직접 통신하는 함수들의 라이브러리
mergeInto(LibraryManager.library, {

    InitFFmpeg: async function() {
        console.log("=== FFmpeg Initialization Called from C# (Class-based Final) ===");
        
        // 1. Script 태그를 동적으로 생성하여 ffmpeg.js 로드
        if (typeof window.FFmpegWASM === 'undefined') {
            const script = document.createElement('script');
            script.src = new URL('StreamingAssets/ffmpeg.js', document.baseURI).href;
            document.head.appendChild(script);

            // 로드가 완료될 때까지 대기
            await new Promise((resolve, reject) => {
                script.onload = resolve;
                script.onerror = reject;
            });
            console.log("✅ ffmpeg.js script loaded.");
        }
        
        try {
            // 2. FFmpegWASM 객체와 그 안의 FFmpeg 클래스가 준비될 때까지 대기
            let timer = 0;
            const timeout = 5000;
            while (!window.FFmpegWASM || !window.FFmpegWASM.FFmpeg) {
                if (timer > timeout) throw new Error("Timeout waiting for FFmpegWASM.FFmpeg class.");
                await new Promise(resolve => setTimeout(resolve, 100));
                timer += 100;
            }

            console.log("✅ FFmpegWASM.FFmpeg class is ready.");
            
            // 3. 올바른 API 사용: new FFmpeg()로 인스턴스 생성
            const { FFmpeg } = window.FFmpegWASM;
            window.ffmpeg = new FFmpeg();

            // FFmpeg 내부 로그를 브라우저 콘솔에 출력하도록 로거 추가
            window.ffmpeg.on('log', ({ type, message }) => {
                console.log(`[FFmpeg log] ${type}: ${message}`);
            });

            // 4. 생성된 인스턴스의 load() 메소드를 호출하여 코어 파일 로드
            const coreURL = new URL('StreamingAssets/ffmpeg-core.js', document.baseURI).href;
            await window.ffmpeg.load({ coreURL });
            
            console.log("✅ FFmpeg MT core loaded and instance created successfully!");
            SendMessage('FFmpegController', 'OnFFmpegReady', 'SUCCESS_CLASS_API');

        } catch (error) {
            console.error("❌ FFmpeg initialization failed:", error);
            SendMessage('FFmpegController', 'OnFFmpegFailed', error.message || 'Unknown final error');
        }
    },

    // startRecording과 stopRecording 함수는 수정할 필요 없습니다.
    startRecording: function(width, height, framerate) {
        try {
            if (!window.ffmpeg || !window.ffmpeg.loaded) { return; } // isLoaded() -> loaded
            const canvas = document.getElementById('unity-canvas');
            if (!canvas) { return; }
            
            console.log(`🎬 [FFmpeg-MT Mode] Starting recording... (${width}x${height} @ ${framerate}fps)`);
            window.recordingStartTime = new Date();
            // C#에서 전달받은 framerate 사용
            const stream = canvas.captureStream(framerate); 
            window.tempRecorder = new MediaRecorder(stream, { mimeType: 'video/webm;codecs=vp8' });
            window.tempChunks = [];
            window.tempRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) window.tempChunks.push(event.data);
            };
            
            window.tempRecorder.onstop = async () => {
                console.log("⏹️ [FFmpeg-MT Mode] Recording stopped. Processing in background...");
                try {
                    if (!window.tempChunks || window.tempChunks.length === 0) throw new Error("No data.");
                    
                    const inputBlob = new Blob(window.tempChunks, { type: 'video/webm' });
                    const inputData = new Uint8Array(await inputBlob.arrayBuffer());
                    await window.ffmpeg.writeFile('input.webm', inputData);

                    const startTime = window.recordingStartTime || new Date();
                    const outputFileName = `rec-mt_${startTime.getFullYear()}.mp4`;
                    
                    console.log(`📝 Starting MT conversion: ${outputFileName}`);
                    
                    // 가장 기본적인 옵션으로 h264 코덱 변환을 다시 시도합니다.
                    await window.ffmpeg.exec([
                        '-i', 'input.webm',
                        // '-c:v', 'copy', // copy 옵션 제거
                        outputFileName
                    ]);
                    
                    const outputData = await window.ffmpeg.readFile(outputFileName);
                    if (outputData.length === 0) throw new Error("Conversion failed.");
                    
                    const outputBlob = new Blob([outputData.buffer], { type: 'video/mp4' });
                    const url = window.URL.createObjectURL(outputBlob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = outputFileName;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);
                    
                    console.log("🎉 MT Video converted and saved:", outputFileName);
                    SendMessage('FFmpegController', 'OnEncodeComplete', 'SUCCESS: ' + outputFileName);
                    
                } catch (error) {
                    SendMessage('FFmpegController', 'OnEncodeComplete', 'FAIL: ' + error.message);
                }
            };
            window.tempRecorder.start();
        } catch (error) { console.error("❌ Error starting MT recording:", error); }
    },
    
    stopRecording: function() {
        if (window.tempRecorder && window.tempRecorder.state === 'recording') {
            window.tempRecorder.stop();
        }
    }
});