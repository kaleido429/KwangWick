using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System;

public class FFmpegController : MonoBehaviour
{
    [Header("Auto Recording Settings")]
    public float autoRecordingDuration = 30f;
    public float recordingStartDelay = 1f;


    [Header("Video Settings")]
    public int videoWidth = 1280; // 1280x720
    public int videoHeight = 720; // 1280x720
    public int frameRate = 30;

    private bool isReady = false;
    private bool isRecording = false;
    private bool autoRecordingCompleted = false;
    [System.Serializable]
    public class FirebaseConfig
    {
        public string apiKey;
        public string authDomain;
        public string projectId;
        public string storageBucket;
        public string messagingSenderId;
        public string appId;
        public string measurementId;
    }

    public GameManager gameManager; // UI 업데이트를 위한 GameManager 참조

    // --- JSLib 함수 선언 ---
    [DllImport("__Internal")]
    private static extern void InitFFmpeg(string FirebaseConfig);

    [DllImport("__Internal")]
    private static extern void startRecording(int width, int height, int framerate);

    [DllImport("__Internal")]
    private static extern void stopRecording();

    [DllImport("__Internal")]
    private static extern void uploadVideo(string videoName);

    [DllImport("__Internal")]
    private static extern void uploadMetadata(string metadata);

    void Start()
    {
        Debug.Log("=== FFmpeg Controller Started (Auto Recording Only) ===");

#if UNITY_WEBGL && !UNITY_EDITOR
        FirebaseConfig config = new FirebaseConfig
        {
            apiKey = FirebaseAPI.apiKey,
            authDomain = FirebaseAPI.authDomain,
            projectId = FirebaseAPI.projectId,
            storageBucket = FirebaseAPI.storageBucket,
            messagingSenderId = FirebaseAPI.messagingSenderId,
            appId = FirebaseAPI.appId,
            measurementId = FirebaseAPI.measurementId
        };
        InitFFmpeg(JsonUtility.ToJson(config));
#endif
    }

    // JSLib에서 호출될 공개 함수 (초기화 성공 시)
    public void OnFFmpegReady(string message)
    {
        Debug.Log($"✅ FFmpeg is ready! Message: {message}");
        isReady = true;

        // 자동 녹화 시작 (조건 없이 바로 실행)
        if (!autoRecordingCompleted)
        {
            StartCoroutine(AutoRecordingSequence());
        }
    }

    // JSLib에서 호출될 공개 함수 (초기화 실패 시)
    public void OnFFmpegFailed(string errorMessage)
    {
        Debug.LogError($"❌ FFmpeg initialization failed. Reason: {errorMessage}");
        isReady = false;
    }

    private IEnumerator AutoRecordingSequence()
    {
        Debug.Log($"Auto recording will start in {recordingStartDelay} seconds for {autoRecordingDuration} seconds");

        // 1단계: 녹화 시작 전 대기
        yield return new WaitForSeconds(recordingStartDelay);

        // 2단계: 녹화 시작
        StartRecording();

        // 3단계: 설정된 시간만큼 녹화
        yield return new WaitForSeconds(autoRecordingDuration);

        // 4단계: 녹화 중단
        StopRecording();

        autoRecordingCompleted = true;
        Debug.Log("✅ Auto recording sequence completed.");
    }

    private void StartRecording()
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
            Debug.Log($"Starting auto recording with {videoWidth}x{videoHeight} @ {frameRate}fps");
            startRecording(videoWidth, videoHeight, frameRate);
            isRecording = true;
#endif
    }

    private void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Not currently recording.");
            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("Stopping auto recording...");
            stopRecording();
            isRecording = false;
#endif
    }

    // JSLib에서 변환 완료 후 호출될 함수
    public void OnEncodeComplete(string result)
    {
        Debug.Log("result: " + result);
        if (result == "SUCCESS")
        {
            //UUID 생성
            Guid uuid = Guid.NewGuid();
            Debug.Log("Generated UUID: " + uuid);
            uploadVideo(uuid.ToString()); // UUID를 비디오 이름으로 사용
        }
    }
 
    

    public void metadata(int peekingHits, int movingHits, int finalScore, int totalShots, int totalHits, double accuracy, int totalHeadshots)
    {
        string metaData = $"{{\"peekingHits\":{peekingHits},\"movingHits\":{movingHits},\"finalScore\":{finalScore},\"totalShots\":{totalShots},\"totalHits\":{totalHits},\"accuracy\":{accuracy},\"totalHeadshots\":{totalHeadshots}}}";
        Debug.Log("metadata: " + metaData);
        uploadMetadata(metaData);
    }

    public void UploadComplete(string message)
    {
        if (message == "SUCCESS")
        {
            gameManager.IsUploadSuccess(true);
        }
        else
        {
            gameManager.IsUploadSuccess(false);
        }
    }

}
