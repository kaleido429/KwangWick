using UnityEngine;

public class PlayerVFXManager : MonoBehaviour
{
    /* 
     * [플레이어의 시각 효과를 관리하는 스크립트입니다.]
     * 총구 화염 효과를 재생합니다.
     */

    #region Variables

    [field: SerializeField] public ParticleSystem MuzzleFlash { get; set; }

    #endregion

    #region User Methods

    // 총구 화염 효과를 재생합니다.
    public void PlayMuzzleFlash()
    {
        if (MuzzleFlash != null)
        {
            MuzzleFlash.Play();
        }
    }

    #endregion

    #region Unity Methods

    public void Start()
    {
        if (MuzzleFlash == null)
        {
            MuzzleFlash = transform.GetComponentInChildren<ParticleSystem>();
        }
    }

    public void OnValidate()
    {
        if (MuzzleFlash == null)
        {
            MuzzleFlash = transform.GetComponentInChildren<ParticleSystem>();
        }
    }

    #endregion
}
