// FFmpegController.cs (새로운 구조)

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;

public class FFmpegController : MonoBehaviour
{
    [Header("Auto Recording Settings")]
    public bool enableAutoRecording = true;
    public float autoRecordingDuration = 3f;
    public float recordingStartDelay = 2f;
    
    private bool isReady = false;
    private bool isRecording = false;
    private bool autoRecordingCompleted = false;

    // --- JSLib 함수 선언 ---
    [DllImport("__Internal")]
    private static extern void InitFFmpeg(); // 초기화 함수 직접 호출

    [DllImport("__Internal")]
    private static extern void startRecording();
    
    [DllImport("__Internal")]
    private static extern void stopRecording();

    void Start()
    {
        Debug.Log("=== FFmpeg Controller Started (New Arch) ===");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            // JSLib에 초기화 시작 신호를 보냄
            InitFFmpeg();
        #else
            Debug.Log("Editor mode - FFmpeg test skipped. Simulating success for testing.");
            // 에디터 테스트를 위해 강제로 준비 완료 상태로 만듦
            OnFFmpegReady("Editor Test Success");
        #endif
    }
    
    // JSLib에서 호출될 공개 함수 (초기화 성공 시)
    public void OnFFmpegReady(string message)
    {
        Debug.Log($"✅ FFmpeg is ready! Message: {message}");
        isReady = true;
        
        // 자동 녹화 시작
        if (enableAutoRecording && !autoRecordingCompleted)
        {
            StartCoroutine(StartAutoRecordingSequence());
        }
    }

    // JSLib에서 호출될 공개 함수 (초기화 실패 시)
    public void OnFFmpegFailed(string errorMessage)
    {
        Debug.LogError($"❌ FFmpeg initialization failed. Reason: {errorMessage}");
        isReady = false;
    }
    
    private IEnumerator StartAutoRecordingSequence()
    {
        Debug.Log($"Auto recording will start in {recordingStartDelay} seconds for {autoRecordingDuration} seconds");
        yield return new WaitForSeconds(recordingStartDelay);
        
        StartManualRecording();
        
        yield return new WaitForSeconds(autoRecordingDuration);
        
        StopManualRecording();
        autoRecordingCompleted = true;
    }

    // --- 수동 녹화 제어 ---
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
            Debug.Log("Calling startRecording...");
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
            Debug.Log("Calling stopRecording...");
            stopRecording();
            isRecording = false;
        #endif
    }

    // JSLib에서 변환 완료 후 호출될 함수
    public void OnEncodeComplete(string result)
    {
        Debug.Log($">>> Encode Complete: {result}");
    }
}