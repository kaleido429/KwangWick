using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{

    // float alphaThreshold = 0.1f;
    // private void Start()
    // {
    //     GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    // }

    public GameObject tutorialPanel;
    public GameObject creditPanel;

    void Start()
    {
        tutorialPanel.SetActive(false);
        creditPanel.SetActive(false);
    }

    public void GameStart()
    {
        SceneManager.LoadScene("Gameplay");
    }

    public void Startbtn()
    {
        tutorialPanel.SetActive(true);
    }

    public void ExitBtn()
    {
        Application.Quit();
        Debug.Log("게임 종료됨.");
    }

    public void CreditBtn()
    {
        creditPanel.SetActive(true);
    }

    public void CloseCreditBtn()
    {
        creditPanel.SetActive(false);
    }

}
