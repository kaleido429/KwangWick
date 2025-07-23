using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

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
    [SerializeField] private TMP_Text peekingHitText;
    [SerializeField] private TMP_Text movingHitText;
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
    private bool isUploadSuccess = false; // 업로드 성공 여부

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
        min = playTimeInSeconds / 60;               // 플레이 시간 분 단위로 변환
        sec = playTimeInSeconds % 60;               // 남은 초 계산
    }

    public void GameEnd()
    {
        state = GameState.End;
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
        peekingHits = ScoreManager.Instance.GetPeekingTargetsHit();
        movingHits = ScoreManager.Instance.GetMovingTargetsHit();

        // 결과 패널 활성화 및 텍스트 설정
        resultPanel.SetActive(true);
        finalScoreText.gameObject.SetActive(false);
        accuracyText.gameObject.SetActive(false);
        headshotsText.gameObject.SetActive(false);
        peekingHitText.gameObject.SetActive(false);
        movingHitText.gameObject.SetActive(false);
        uploadButton.gameObject.SetActive(false);

        // 결과 텍스트 설정
        finalScoreText.text = $"Final Score : {finalScore}";
        accuracyText.text = $"Accuracy : {totalHits}/{totalShots} ({accuracy:F2}%)";
        headshotsText.text = $"Headshots : {totalHeadshots}";
        peekingHitText.text = $"Peeking Target Hit : {peekingHits}";
        movingHitText.text = $"Moving Target Hit : {movingHits}";

        // FFmpegController의 upload 메서드를 호출하여 비디오 업로드 -> 영상 녹화 끝나자마자 업로드
        ffmpegController.metadata(peekingHits, movingHits, finalScore, totalShots, totalHits, System.Math.Round(accuracy, 2), totalHeadshots);

        // 텍스트 효과를 적용하여 결과 텍스트 표시
        StartCoroutine(TypeTextEffect(0.1f));       // 10초 정도 딜레이
    }

    public IEnumerator TypeSingleText(TMP_Text textComponent, string text, float delay)
    {
        textComponent.text = "";                    // 텍스트 초기화
        textComponent.gameObject.SetActive(true);   // 텍스트 컴포넌트 활성화
        StringBuilder stringBuilder = new();

        foreach (char c in text)
        {
            stringBuilder.Append(c);                        // 현재 문자 추가
            textComponent.text = stringBuilder.ToString();  // 텍스트 업데이트
            yield return new WaitForSeconds(delay);         // 딜레이 적용
        }
    }

    public IEnumerator TypeTextEffect(float delay)
    {
        // 각 텍스트 컴포넌트에 대해 타이핑 효과 적용
        yield return TypeSingleText(finalScoreText, finalScoreText.text, delay);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        yield return TypeSingleText(accuracyText, accuracyText.text, delay);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        yield return TypeSingleText(headshotsText, headshotsText.text, delay);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        yield return TypeSingleText(peekingHitText, peekingHitText.text, delay);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        yield return TypeSingleText(movingHitText, movingHitText.text, delay);
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        // 비디오 업로드 버튼 활성화
        uploadButton.gameObject.SetActive(true);
        uploadButton.interactable = false;
    }

    public void UploadVideoOnGameEnd()
    {
        if(isUploadSuccess == false)
        {
            // 업로드 실패 시 다시 시도 가능
            ffmpegController.metadata(peekingHits, movingHits, finalScore, totalShots, totalHits, System.Math.Round(accuracy, 2), totalHeadshots);
        }

        // 업로드 후 버튼 비활성화
        uploadButton.interactable = false;

        // 비디오 업로드 중 표시
        uploadButton.GetComponentInChildren<TMP_Text>().text = "Uploading Video... Please Wait";
    }

    public void IsUploadSuccess(bool isSuccess)
    {
        if (isSuccess)
        {
            // 비디오 업로드 성공
            isUploadSuccess = true;             // 업로드 성공 상태 설정
            uploadButton.interactable = false;
            uploadButton.GetComponentInChildren<TMP_Text>().text = "Upload Success";
        }
        else
        {
            // 비디오 업로드 실패
            isUploadSuccess = false;            // 업로드 실패 상태 설정
            uploadButton.interactable = true;   // 다시 업로드 시도 가능
            uploadButton.GetComponentInChildren<TMP_Text>().text = "Upload Failed. Try Again";
        }
    }

}
