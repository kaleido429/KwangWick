using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerManager : MonoBehaviour
{
    /*
     * [플레이어 전반에 관한 스크립트입니다.]
     * 여러 스크립트간의 의존성을 줄이기 위해 PlayerManager를 사용합니다.
     */

    #region Variables

    [Tooltip("컴포넌트")]
    [field: SerializeField, Header("Components")] public PlayerInfo PlayerInfo { get; set; }
    [field: SerializeField] public PlayerInput PlayerInput { get; set; }
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    [field: SerializeField] public FirstPersonCameraController CameraController { get; set; }
    [field: SerializeField] public Camera PlayerCamera { get; set; }

    #endregion

    #region Unity Methods

    public void Start()
    {
        if (PlayerInfo == null)
        {
            PlayerInfo = GetComponent<PlayerInfo>();
        }
        if (PlayerInput == null)
        {
            PlayerInput = GetComponent<PlayerInput>();
        }
        if (CharacterController == null)
        {
            CharacterController = GetComponent<CharacterController>();
        }
        if (CameraController == null)
        {
            CameraController = GetComponent<FirstPersonCameraController>();
        }
        if (PlayerCamera == null)
        {
            PlayerCamera = GetComponentInChildren<Camera>();
        }
    }

    public void OnValidate()
    {
        if (PlayerInfo == null)
        {
            PlayerInfo = GetComponent<PlayerInfo>();
        }
        if (PlayerInput == null)
        {
            PlayerInput = GetComponent<PlayerInput>();
        }
        if (CharacterController == null)
        {
            CharacterController = GetComponent<CharacterController>();
        }
        if (CameraController == null)
        {
            CameraController = GetComponent<FirstPersonCameraController>();
        }
        if (PlayerCamera == null)
        {
            PlayerCamera = GetComponentInChildren<Camera>();
        }
    }

    #endregion
}
