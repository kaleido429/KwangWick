mergeInto(LibraryManager.library, {
    // 전역 FFmpeg 인스턴스 및 상태 변수
    ffmpeg: null,
    isReady: false,
    frameCount: 0,

    // C#에서 호출할 초기화 함수
    InitFFmpeg: async function () {
        // 전역 window 객체에 로드된 FFmpeg 라이브러리를 사용합니다.
        if (!window.FFmpeg || !window.FFmpeg.createFFmpeg) {
            console.error("FFmpeg.js has not been loaded yet. Check your WebGL template.");
            return;
        }

        const { createFFmpeg } = window.FFmpeg;
        
        window.ffmpeg = createFFmpeg({
            // WebGL 빌드 시 파일 경로는 빌드 폴더의 루트를 기준으로 합니다.
            // Plugins/WebGL 폴더의 파일들은 빌드 후 루트에 복사됩니다.
            corePath: 'ffmpeg-core.js', 
            log: true 
        });

        try {
            await window.ffmpeg.load();
            window.isReady = true;
            console.log("FFmpeg is loaded and ready.");
            // 초기화가 완료되면 유니티의 'FFmpegController' 오브젝트의 'OnFFmpegReady' 메소드로 메시지를 보냅니다.
            SendMessage('FFmpegController', 'OnFFmpegReady', 'FFmpeg is Ready!');
        } catch (e) {
            console.error("Error loading FFmpeg:", e);
            SendMessage('FFmpegController', 'OnFFmpegReady', 'Error: ' + e);
        }
    },

    // C#에서 캡처한 프레임 데이터를 받아 가상 파일 시스템에 저장하는 함수
    AddFrame: function (dataPtr, dataLength, fileNamePtr) {
        if (!window.isReady || !window.ffmpeg) return;

        // C#에서 보낸 byte[] 데이터(포인터와 길이)를 JavaScript의 Uint8Array로 변환합니다.
        const frameData = new Uint8Array(HEAPU8.subarray(dataPtr, dataPtr + dataLength));
        const fileName = UTF8ToString(fileNamePtr);

        try {
            // ffmpeg의 가상 파일 시스템에 'frame-0001.png' 같은 이름으로 파일을 씁니다.
            window.ffmpeg.FS('writeFile', fileName, frameData);
            window.frameCount++;
        } catch(e) {
            console.error("Failed to write frame:", e);
        }
    },

    // C#에서 녹화 종료 및 인코딩을 명령하는 함수
    EndRecordingAndEncode: async function (outputNamePtr, framerate) {
        if (!window.isReady || !window.ffmpeg || window.frameCount == 0) return;
        
        const outputName = UTF8ToString(outputNamePtr);
        console.log(`Encoding ${window.frameCount} frames to ${outputName} at ${framerate}fps...`);

        // FFmpeg 인코딩 명령어를 실행합니다.
        await window.ffmpeg.run(
            '-framerate', String(framerate),
            '-i', 'frame-%04d.png',      // 입력 파일 이름 패턴
            '-c:v', 'libx264',           // H.264 비디오 코덱 사용
            '-pix_fmt', 'yuv420p',       // 플레이어 호환성을 위한 픽셀 포맷
            outputName
        );

        console.log("Encoding complete.");
        
        // 결과 비디오 파일을 가상 파일 시스템에서 읽어옵니다.
        const data = window.ffmpeg.FS('readFile', outputName);
        
        // 결과 파일을 사용자의 컴퓨터에 다운로드시키는 함수를 호출합니다.
        DownloadFile(data, outputName, 'video/mp4');

        // 녹화 상태를 초기화합니다.
        window.frameCount = 0;
        // 인코딩이 완료되었음을 유니티로 알립니다.
        SendMessage('FFmpegController', 'OnEncodeComplete', outputName);
    }
});

// 파일 다운로드 헬퍼 함수
function DownloadFile(data, filename, mimetype) {
    const blob = new Blob([data.buffer], { type: mimetype });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    document.body.appendChild(a);
    a.style = 'display: none';
    a.href = url;
    a.download = filename;
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
}