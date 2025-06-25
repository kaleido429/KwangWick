using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FFmpegController : MonoBehaviour
{
    // jslib 파일에 정의한 JavaScript 함수들을 C#에서 인식하도록 선언합니다.
    [DllImport("__Internal")]
    private static extern void InitFFmpeg();

    [DllImport("__Internal")]
    private static extern void AddFrame(byte[] data, int dataLength, string fileName);

    [DllImport("__Internal")]
    private static extern void EndRecordingAndEncode(string outputName, int framerate);

    // 상태를 표시할 Text UI만 남겨둡니다.
    public Text statusText;

    private bool isRecording = false;
    private int frameCount = 0;
    private int framerate = 30; // 30fps로 녹화

    void Start()
    {
        // 초기 상태 설정
        statusText.text = "Initializing FFmpeg...";

        // WebGL 빌드에서만 JavaScript 함수를 호출합니다.
        #if UNITY_WEBGL && !UNITY_EDITOR
            InitFFmpeg();
        #else
            statusText.text = "Run in a WebGL build to test auto-recording.";
        #endif
    }
    
    // --- JavaScript로부터 메시지를 받는 메소드들 ---
    
    // [변경점] FFmpeg가 준비되면, 바로 10초 녹화 코루틴을 시작합니다.
    public void OnFFmpegReady(string message)
    {
        Debug.Log(message); // "FFmpeg is Ready!"
        statusText.text = "FFmpeg Ready. Starting auto-record...";
        StartCoroutine(RecordForDuration(10.0f)); // 10초 동안 녹화 시작
    }

    public void OnEncodeComplete(string filename)
    {
        Debug.Log($"Encoding complete: {filename}");
        statusText.text = $"10-second recording complete! {filename} downloaded.";
    }
    
    // [추가] 지정된 시간 동안 녹화를 진행하는 전체 과정을 관리하는 코루틴
    private IEnumerator RecordForDuration(float duration)
    {
        // 1. 녹화 시작
        StartRecording();
        
        // 2. 지정된 시간(duration)만큼 대기
        yield return new WaitForSeconds(duration);

        // 3. 녹화 종료
        StopRecording();
    }

    // --- 내부 로직 메소드들 (기존과 거의 동일) ---
    private void StartRecording()
    {
        if (isRecording) return;
        
        isRecording = true;
        frameCount = 0;
        statusText.text = "Auto-Recording for 10 seconds...";
        StartCoroutine(CaptureFrames());
    }

    private void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        statusText.text = "Encoding... Please wait.";
        Debug.Log("10 seconds elapsed. Stopping recording and starting encode.");

        #if UNITY_WEBGL && !UNITY_EDITOR
            EndRecordingAndEncode("auto-record-10s.mp4", framerate);
        #endif
    }
    
    // 화면 프레임을 주기적으로 캡처하는 코루틴 (기존과 동일)
    private IEnumerator CaptureFrames()
    {
        while (isRecording)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            byte[] data = texture.EncodeToPNG();
            Destroy(texture);
            string fileName = $"frame-{frameCount:D4}.png";

            #if UNITY_WEBGL && !UNITY_EDITOR
                AddFrame(data, data.Length, fileName);
            #endif
            frameCount++;

            yield return new WaitForSecondsRealtime(1f / framerate);
        }
    }
}