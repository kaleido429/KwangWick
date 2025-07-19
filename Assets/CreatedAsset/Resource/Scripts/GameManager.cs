using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum GameState
{
    Intro,
    Playing,
    End
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState state;
    public int playTimeInSeconds = 60; // 게임 플레이 시간 (초 단위)
    public GameObject countdownPanel;
    public GameObject resultPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text headshotsText;
    [SerializeField] private FFmpegController ffmpegController;
    [SerializeField] private Button uploadButton;

    public GameObject[] objectsToDisable; // 끌 오브젝트 배열
    public GameObject playerGameObject;
    private PlayerInput playerInput;

    private float sec;
    private int min;
    private int peekingHits;
    private int movingHits;
    private int finalScore;
    private int totalShots;
    private int totalHits;
    private double accuracy;
    private int totalHeadshots;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if(playerGameObject == null)
        {
            playerGameObject = GameObject.FindWithTag("Player");
        }
        if(playerGameObject != null)
        {
            playerInput = playerGameObject.GetComponent<PlayerInput>();
        }
        StartCoroutine(CountdownToStart()); // 게임 시작 카운트다운 시작
    }

    void Update()
    {
        if (state == GameState.Playing)
        {
            // 타이머 감소
            sec -= Time.deltaTime;

            // 초가 0보다 작아질 경우
            if (sec < 0)
            {
                if (min > 0)
                {
                    min -= 1;
                    sec += 60f;
                }
                else
                {
                    sec = 0;
                    GameEnd();
                }
            }

            // 화면에 시간 표시
            timeText.text = string.Format("{0:D2}:{1:D2}", min, (int)sec);
        }
    }

    IEnumerator CountdownToStart()
    {
        playerInput.SetInputActive(false);
        countdownPanel.SetActive(true);
        resultPanel.SetActive(false);

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);    // 1초 대기
        }

        countdownText.text = "Start!";
        yield return new WaitForSeconds(0.5f);      // "Start!" 표시 후 0.5초 대기

        state = GameState.Playing;                  // 게임 시작
        playerInput.SetInputActive(true);           // 플레이어 입력 활성화
        countdownPanel.SetActive(false);
        min = playTimeInSeconds / 60; // 플레이 시간 분 단위로 변환
        sec = playTimeInSeconds % 60; // 남은 초 계산
    }

    public void GameEnd()
    {
        state = GameState.End;
        Time.timeScale = 0f;                // 게임 일시정지
        playerInput.SetInputActive(false);  // 플레이어 입력 비활성화

        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        finalScore = ScoreManager.Instance.GetScore();
        totalShots = ScoreManager.Instance.GetShots();
        totalHits = ScoreManager.Instance.GetHits();
        totalHeadshots = ScoreManager.Instance.GetHeadshots();
        accuracy = totalShots > 0 ? (double)totalHits / totalShots * 100 : 0;

        resultPanel.SetActive(true);
        finalScoreText.text = $"Final Score: {finalScore}";
        accuracyText.text = $"Accuracy: {totalHits}/{totalShots} ({accuracy:F2}%)";
        headshotsText.text = $"Headshots: {totalHeadshots}";
        // Peeking이랑 Moving 타겟의 히트 수를 표시할 수 있다면 추가로 표시
        peekingHits = ScoreManager.Instance.GetPeekingTargetsHit();
        //Debug.Log($"Peeking Hits: {peekingHits}");
        movingHits = ScoreManager.Instance.GetMovingTargetsHit();
        //Debug.Log($"Moving Hits: {movingHits}");

        // 비디오 업로드 버튼 활성화
        uploadButton.interactable = true;
    }

    public void UploadVideoOnGameEnd()
    {
        // 비디오 업로드 버튼이 클릭되면 FFmpegController의 upload 메서드를 호출
        ffmpegController.upload(peekingHits, movingHits, finalScore, totalShots, totalHits, System.Math.Round(accuracy, 2), totalHeadshots);

        // 업로드 후 버튼 비활성화
        uploadButton.interactable = false;

        Debug.Log("비디오 업로드 요청이 전송되었습니다.");

        // 비디오 업로드 중 표시
        uploadButton.GetComponentInChildren<TMP_Text>().text = "Uploading Video... Please Wait";
    }

    public void IsUploadSuccess(bool isSuccess)
    {
        if (isSuccess)
        {
            // 비디오 업로드 성공
            uploadButton.interactable = false;
            uploadButton.GetComponentInChildren<TMP_Text>().text = "Upload Success";
        }
        else
        {
            // 비디오 업로드 실패
            uploadButton.interactable = true; // 다시 업로드 시도 가능
            uploadButton.GetComponentInChildren<TMP_Text>().text = "Upload Failed";
        }
    }

}
