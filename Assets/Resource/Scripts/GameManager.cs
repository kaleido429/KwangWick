using UnityEngine.UI;
using UnityEngine;
using TMPro;

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
    public GameObject resultPanel;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text finalScoreText;
    private float sec;
    private int min;

    public GameObject[] objectsToDisable; // 끌 오브젝트 배열

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        state = GameState.Playing;
        resultPanel.SetActive(false);
        sec = 0f;
        min = 1;
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

        resultPanel.SetActive(true);
        finalScoreText.text = $"Final Score: {ScoreManager.Instance.GetScore()}";
    }

}
