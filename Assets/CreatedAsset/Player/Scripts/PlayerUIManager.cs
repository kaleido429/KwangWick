using UnityEngine;
using System.Collections;

public class PlayerUIManager : MonoBehaviour
{
    /* 
     * [플레이어 UI를 관리하는 스크립트입니다.]
     * 플레이어의 UI 요소들을 활성화하거나 비활성화합니다.
     */

    #region Variables

    [field: SerializeField] public GameObject HitBody { get; set; }
    [field: SerializeField] public GameObject HitHead { get; set; }
    [field: SerializeField] public GameObject HitKill { get; set; }

    #endregion

    #region User Methods

    // 타켓이 맞았을 때 UI를 토글합니다.
    public void ToggleHitUI(int hitType)
    {
        switch (hitType)
        {
            case 1: // HitType.Body
                StartCoroutine(ShowAndHideHitUI(HitBody));
                break;
            case 2: // HitType.Head
                StartCoroutine(ShowAndHideHitUI(HitHead));
                break;
            case 3: // HitType.Kill
                StartCoroutine(ShowAndHideHitUI(HitKill));
                break;
            default: // HitType.None
                break;
        }
    }

    // UI를 보여주고 숨기는 코루틴입니다.
    IEnumerator ShowAndHideHitUI(GameObject hitUI, float duration = 0.2f)
    {
        hitUI.SetActive(true);
        yield return new WaitForSeconds(duration);
        hitUI.SetActive(false);
    }

    #endregion

    #region Unity Methods

    public void Start()
    {
        if (HitBody == null)
        {
            HitBody = transform.Find("HitMarker/Body").gameObject;
        }
        if (HitHead == null)
        {
            HitHead = transform.Find("HitMarker/Head").gameObject;
        }
        if (HitKill == null)
        {
            HitKill = transform.Find("HitMarker/Kill").gameObject;
        }
    }

    public void OnValidate()
    {
        if (HitBody == null)
        {
            HitBody = transform.Find("HitMarker/Body").gameObject;
        }
        if (HitHead == null)
        {
            HitHead = transform.Find("HitMarker/Head").gameObject;
        }
        if (HitKill == null)
        {
            HitKill = transform.Find("HitMarker/Kill").gameObject;
        }
    }

    #endregion
}
