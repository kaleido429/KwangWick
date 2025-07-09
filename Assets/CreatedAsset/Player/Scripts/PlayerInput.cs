using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FirstPersonCameraController))]
public class PlayerInput : MonoBehaviour
{
    /*
     * [플레이어의 입력을 처리하는 스크립트입니다.]
     * 마우스 이동, 클릭, Q, E를 받아서 처리합니다.
     */

    #region Variables

    [Tooltip("컴포넌트")]
    [field: SerializeField, Header("Components")] FirstPersonCameraController CameraController { get; set; }
    [field: SerializeField] Animator Animator { get; set; }
    [field: SerializeField] SoundManager SoundManager { get; set; }
    [field: SerializeField] Gun Gun { get; set; }
    [field: SerializeField] PlayerUIManager PlayerUIManager { get; set; }
    [field: SerializeField] PlayerVFXManager PlayerVFXManager { get; set; }
    [field: SerializeField, Header("Controls")] public bool IsInputEnabled { get; set; } = false;

    #endregion

    #region Input Handlers

    // 마우스 이동을 처리합니다.
    public void OnLook(InputValue value)
    {
        if (IsInputEnabled == false) return;
        CameraController.LookInput = value.Get<Vector2>();
    }

    // 마우스 왼쪽 클릭을 처리합니다.
    public void OnAttack()
    {
        if (IsInputEnabled == false) return;
        Animator.SetTrigger("Fire");
        SoundManager.GunFire();
        CameraController.ApplyRecoil();
        PlayerVFXManager.PlayMuzzleFlash();

        if (Gun != null)
        {
            var (hitType,isPeeking) = Gun.Shoot();

            // HitType에 따라 UI를 토글합니다.
            PlayerUIManager.ToggleHitUI(hitType);

            // 히트 기록을 추가합니다.
            //if (hitType > 0) ScoreManager.Instance.AddHit(isPeeking); Hit수 관리 Target.cs에서 관리
            if(hitType == (int)HitType.Head) ScoreManager.Instance.AddHeadshot();
        }

        // 총알 발사 기록을 추가합니다.
        ScoreManager.Instance.AddShot();
    }

    // Q 키를 처리합니다.
    public void OnLeanLeft(InputValue value)
    {
        if (IsInputEnabled == false) return;
        CameraController.LeanLeftToggle = value.Get<float>();
    }

    // E 키를 처리합니다.
    public void OnLeanRight(InputValue value)
    {
        if (IsInputEnabled == false) return;
        CameraController.LeanRightToggle = value.Get<float>();
    }

    // 입력 활성화 상태를 설정합니다.
    public void SetInputActive(bool active)
    {
        IsInputEnabled = active;
    }

    #endregion

    #region Unity Methods

    public void Start()
    {
        if(CameraController == null)
        {
            CameraController = GetComponent<FirstPersonCameraController>();
        }
        if(Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
        }
        if(SoundManager == null)
        {
            SoundManager = GetComponentInChildren<SoundManager>();
        }
        if(Gun == null)
        {
            Gun = GetComponentInChildren<Gun>();
        }
        if (PlayerUIManager == null)
        {
            PlayerUIManager = GetComponentInChildren<PlayerUIManager>();
        }
        if (PlayerVFXManager == null)
        {
            PlayerVFXManager = GetComponentInChildren<PlayerVFXManager>();
        }
    }

    public void OnValidate()
    {
        if (CameraController == null)
        {
            CameraController = GetComponent<FirstPersonCameraController>();
        }
        if (Animator == null)
        {
            Animator = GetComponentInChildren<Animator>();
        }
        if (SoundManager == null)
        {
            SoundManager = GetComponentInChildren<SoundManager>();
        }
        if (Gun == null)
        {
            Gun = GetComponentInChildren<Gun>();
        }
        if (PlayerUIManager == null)
        {
            PlayerUIManager = GetComponentInChildren<PlayerUIManager>();
        }
        if (PlayerVFXManager == null)
        {
            PlayerVFXManager = GetComponentInChildren<PlayerVFXManager>();
        }
    }

    #endregion
}
