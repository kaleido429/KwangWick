using TMPro;
using UnityEngine;
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
    public GameObject countdownPanel;
    public GameObject resultPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text headshotText;
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
        Time.timeScale = 0f;    // 게임 일시정지

        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        resultPanel.SetActive(true);
        int finalScore = ScoreManager.Instance.GetScore();
        int hits = ScoreManager.Instance.GetHits();
        int shots = ScoreManager.Instance.GetShots();
        double accuracy = shots > 0 ? (double)hits / shots * 100 : 0;
        int headshots = ScoreManager.Instance.GetHeadshots();

        finalScoreText.text = $"Final Score : {finalScore}";
        accuracyText.text = $"Accuracy : {hits}/{shots} ({accuracy:F2}%)";
        headshotText.text = $"Headshots : {headshots}";
    }

}
