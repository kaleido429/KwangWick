using TMPro;
using UnityEngine;
using System.Collections;
using System;

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
    public GameObject countdownPanel;
    public GameObject resultPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text headshotsText;
    [SerializeField] private FFmpegController ffmpegController;
    private float sec;
    private int min;

    public GameObject[] objectsToDisable; // 끌 오브젝트 배열

    public GameObject playerGameObject;
    private PlayerInput playerInput;

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
        sec = 0f;
        min = 1;
    }

    public void GameEnd()
    {
        state = GameState.End;
        Time.timeScale = 0f; // 게임 일시정지

        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        int finalScore = ScoreManager.Instance.GetScore();
        int totalShots = ScoreManager.Instance.GetShots();
        int totalHits = ScoreManager.Instance.GetHits();
        int totalHeadshots = ScoreManager.Instance.GetHeadshots();
        double accuracy = totalShots > 0 ? (double)totalHits / totalShots * 100 : 0;

        resultPanel.SetActive(true);
        finalScoreText.text = $"Final Score: {finalScore}";
        accuracyText.text = $"Accuracy: {totalHits}/{totalShots} ({accuracy:F2}%)";
        headshotsText.text = $"Headshots: {totalHeadshots}";
        // Peeking이랑 Moving 타겟의 히트 수를 표시할 수 있다면 추가로 표시
        int peekingHits = ScoreManager.Instance.GetPeekingTargetsHit();
        //Debug.Log($"Peeking Hits: {peekingHits}");
        int movingHits = ScoreManager.Instance.GetMovingTargetsHit();
        //Debug.Log($"Moving Hits: {movingHits}");
        ffmpegController.upload(peekingHits, movingHits, finalScore, totalShots, totalHits, System.Math.Round(accuracy, 2), totalHeadshots);
    }

}
