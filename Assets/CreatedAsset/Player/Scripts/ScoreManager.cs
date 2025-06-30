using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int score = 0;
    private int shots = 0;
    private int hits = 0;
    private int headshots = 0;
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

    public void AddHit()
    {
        hits++;
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

}
