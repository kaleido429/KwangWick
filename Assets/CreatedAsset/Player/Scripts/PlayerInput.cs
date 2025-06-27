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

    #endregion

    #region Input Handlers

    // 마우스 이동을 처리합니다.
    public void OnLook(InputValue value)
    {
        CameraController.LookInput = value.Get<Vector2>();
    }

    // 마우스 왼쪽 클릭을 처리합니다.
    public void OnAttack()
    {
        Animator.SetTrigger("Fire");
        SoundManager.GunFire();
        CameraController.ApplyRecoil();
        PlayerVFXManager.PlayMuzzleFlash();

        if (Gun != null)
        {
            int hitType = Gun.Shoot();

            // HitType에 따라 UI를 토글합니다.
            PlayerUIManager.ToggleHitUI(hitType);
        }
    }

    // Q 키를 처리합니다.
    public void OnLeanLeft(InputValue value)
    {
        CameraController.LeanLeftToggle = value.Get<float>();
    }

    // E 키를 처리합니다.
    public void OnLeanRight(InputValue value)
    {
        CameraController.LeanRightToggle = value.Get<float>();
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
