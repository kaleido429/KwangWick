using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class FFmpegController : MonoBehaviour
{
    [Header("Auto Recording Settings")]
    [Tooltip("Enable auto recording on start")]
    public bool enableAutoRecording = true;
    
    [Tooltip("Auto recording duration (seconds)")]
    [Range(1, 60)]
    public float autoRecordingDuration = 3f;
    
    [Tooltip("Delay before starting recording (seconds)")]
    [Range(0, 10)]
    public float recordingStartDelay = 2f;
    
    // --- 간소화된 상태 변수 ---
    private bool isReady = false;
    private bool isRecording = false;
    private bool autoRecordingCompleted = false;

    // --- 새롭고 간결해진 JavaScript 함수 선언 ---
    [DllImport("__Internal")]
    private static extern bool IsFFmpegReady();

    [DllImport("__Internal")]
    private static extern void startRecording();
    
    [DllImport("__Internal")]
    private static extern void stopRecording();

    void Start()
    {
        Debug.Log("=== FFmpeg Controller Started (MP4 Mode) ===");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(InitializeFFmpegSequence());
        #else
            Debug.Log("Editor mode - FFmpeg test skipped");
        #endif
    }

    private IEnumerator InitializeFFmpegSequence()
    {
        Debug.Log("Waiting for FFmpeg to be ready... (Requires secure server with COOP/COEP headers)");

        float timeout = 30f; // 30초 타임아웃
        float timer = 0f;

        // JavaScript의 isFFmpegReady 플래그가 true가 될 때까지 매 프레임 확인
        while (!IsFFmpegReady())
        {
            if (timer > timeout)
            {
                Debug.LogError("❌ FFmpeg initialization timed out. Ensure you are running on a secure server with COOP/COEP headers.");
                isReady = false;
                yield break; // 코루틴 종료
            }

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 루프가 성공적으로 끝나면 FFmpeg가 준비된 것
        isReady = true;
        Debug.Log("✅ FFmpeg is ready for use!");
        
        // 자동 녹화 시작
        if (enableAutoRecording && !autoRecordingCompleted)
        {
            Debug.Log($"Auto recording will start in {recordingStartDelay} seconds for {autoRecordingDuration} seconds");
            yield return new WaitForSeconds(recordingStartDelay);
            StartAutoRecording();
        }
    }

    private void StartAutoRecording()
    {
        if (isRecording || autoRecordingCompleted) return;
        
        Debug.Log("🔴 Starting auto recording...");
        StartManualRecording();
        
        StartCoroutine(StopAutoRecordingAfterDelay());
        autoRecordingCompleted = true;
    }

    private IEnumerator StopAutoRecordingAfterDelay()
    {
        yield return new WaitForSeconds(autoRecordingDuration);
        Debug.Log("⏹️ Auto recording completed");
        StopManualRecording();
    }

    // --- 간소화된 수동 녹화 제어 ---
    public void StartManualRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("Already recording.");
            return;
        }
        
        if (!isReady)
        {
            Debug.LogError("Cannot start recording: FFmpeg is not ready.");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("Calling startRecording (MP4)...");
            startRecording();
            isRecording = true;
        #endif
    }

    public void StopManualRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Not currently recording.");
            return;
        }
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("Calling stopRecording (MP4 conversion)...");
            stopRecording();
            isRecording = false;
        #endif
    }

    // JavaScript에서 변환 완료 후 호출해 줄 수 있는 함수
    public void OnEncodeComplete(string result)
    {
        Debug.Log($">>> Encode Complete: {result}");
    }
}