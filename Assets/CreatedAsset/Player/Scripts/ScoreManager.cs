using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int score = 0;
    private int shots = 0;
    private int hits = 0;
    private int headshots = 0;
    private int peekingTargetHit = 0;//peeking 타겟 맞은 횟수
    private int movingTargetHit = 0;//움직이는 타겟 맞은 횟수
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void AddShot()
    {
        shots++;
    }

    public void AddHit(bool isPeeking)
    {
        hits++; //전체 hit 증가
        if (isPeeking)
        {
            peekingTargetHit++; //peeking 타겟 맞은 횟수 증가
            Debug.Log("Peeking Target Hit" + peekingTargetHit);
        }
        else
        {
            movingTargetHit++; //움직이는 타겟 맞은 횟수 증가
            Debug.Log("Moving Target Hit" + movingTargetHit);
        }
    }

    public void AddHeadshot()
    {
        headshots++;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetShots()
    {
        return shots;
    }

    public int GetHits()
    {
        return hits;
    }

    public int GetHeadshots()
    {
        return headshots;
    }
    public int GetPeekingTargetsHit() => peekingTargetHit;
    public int GetMovingTargetsHit() => movingTargetHit;
}
