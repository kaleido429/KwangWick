using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonCameraController : MonoBehaviour
{
    /*
     * [일인칭 카메라와 관련된 기능을 구현하는 스크립트입니다.]
     * 회전, 기울이기, 반동 등을 처리합니다.
     */

    #region Variables

    [Tooltip("카메라 설정")]
    [field: SerializeField, Header("Camera Settings")] public float CameraSensitivity { get; set; } = 50f;
    [field: SerializeField] public Vector3 CameraOffset { get; set; } = new Vector3(0f, 1.63f, 0f);
    [field: SerializeField] public Quaternion CameraRotation { get; set; } = new Quaternion(0f, 0f, 0f, 1f);
    [field: SerializeField] public float CameraFOV { get; set; } = 75f;
    [field: SerializeField] public float MaxPitchAngle { get; set; } = 90f;
    [field: SerializeField] public float MinPitchAngle { get; set; } = -90f;
    public float CurrentPitch { get => CurrentPitchAngle; set { CurrentPitchAngle = Mathf.Clamp(value, MinPitchAngle, MaxPitchAngle); } }
    [field: SerializeField] public bool LockCursor { get; set; } = true;
    [field: SerializeField] public bool InvertX { get; set; } = false;
    [field: SerializeField] public bool InvertY { get; set; } = false;

    [Tooltip("카메라 효과")]
    [field: SerializeField, Header("Camera Effects")] public float MaxLeanAngle { get; set; } = 8f;
    [field: SerializeField] public float LeanSpeed { get; set; } = 10f;
    [field: SerializeField] public float LeanOffset { get; set; } = 0.5f;
    [field: SerializeField] public bool UseLean { get; set; } = true;
    [field: SerializeField] public bool ToggleLean { get; set; } = false;
    [field: SerializeField] public float RecoilReturnSpeed { get; set; } = 2f;
    [field: SerializeField] public float RecoilDampTime { get; set; } = 6f;
    [field: SerializeField] public Vector3 RecoilAmount { get; set; } = new(8f, 0.01f, 0f);
    [field: SerializeField] public bool UseCameraRecoil { get; set; } = true;

    [Tooltip("디버그: 카메라 계산 결과")]
    [field: SerializeField, Header("Camera Result")] public Vector3 ResultLeanPosition { get; set; } = Vector3.zero;
    [field: SerializeField] public Vector3 ResultLeanRotation { get; set; } = Vector3.zero;
    [field: SerializeField] public float CurrentPitchAngle { get; set; } = 0f;
    [field: SerializeField] public float CurrentLeanAngle { get; set; } = 0f;
    [field: SerializeField] public Vector3 ResultRecoilRotation { get; set; } = Vector3.zero;
    [field: SerializeField] public Vector3 CurrentRecoil { get; set; } = Vector3.zero;

    [Tooltip("디버그: 플레이어 입력")]
    [field: SerializeField, Header("Player Input")] public Vector2 LookInput { get; set; } = Vector2.zero;
    [field: SerializeField] public float LeanLeft { get; set; } = 0f;
    public float LeanLeftToggle { get => LeanLeft; set { if (ToggleLean && value != 0f) LeanLeft = Mathf.Abs(LeanLeft - value); else if (!ToggleLean) LeanLeft = value; } }
    [field: SerializeField] public float LeanRight { get; set; } = 0f;
    public float LeanRightToggle { get => LeanRight; set { if (ToggleLean && value != 0f) LeanRight = Mathf.Abs(LeanRight - value); else if (!ToggleLean) LeanRight = value; } }

    [Tooltip("컴포넌트")]
    [field: SerializeField, Header("Components")] Camera PlayerCamera { get; set; }
    [field: SerializeField] CharacterController CharacterController { get; set; }
    [field: SerializeField] CameraManager CameraManager { get; set; }
    [field: SerializeField] MeshManager MeshManager { get; set; }

    #endregion

    #region Camera Methods

    // 카메라 설정을 업데이트합니다.
    private void CameraSettingsUpdate()
    {
        // FOV 설정을 업데이트합니다.
        PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, CameraFOV, Time.deltaTime);
    }

    // 카메라 기울이기를 업데이트합니다.
    private void LeanUpdate()
    {
        if (!UseLean) return;

        // 기울이기 입력이 동시에 들어올 경우 둘 다 0으로 설정합니다.
        if (LeanLeft != 0f && LeanRight != 0f)
        {
            LeanLeft = 0f;
            LeanRight = 0f;
        }

        // 기울이기 입력에 따라 카메라 회전 각도와 위치 오프셋을 계산합니다.
        float rotationAngle = (LeanLeft + (-1f * LeanRight)) * MaxLeanAngle;
        Vector3 positionOffset = ((-1f * LeanLeft) + LeanRight) * new Vector3(LeanOffset, 0f, 0f);

        // 기울이기 각도와 위치를 보간하여 부드럽게 전환합니다.
        CurrentLeanAngle = Mathf.Lerp(CurrentLeanAngle, rotationAngle, Time.deltaTime * LeanSpeed);
        ResultLeanPosition = Vector3.Lerp(ResultLeanPosition, positionOffset, Time.deltaTime * LeanSpeed);
        ResultLeanRotation = new Vector3(0f, 0f, CurrentLeanAngle);
    }

    // 카메라 회전을 업데이트합니다.
    private void LookUpdate()
    {
        // 카메라 회전 입력을 가져와서 회전 각도를 계산합니다.
        Vector2 lookInput = CameraSensitivity * MouseSettings.sensitivity * Time.deltaTime * new Vector2(LookInput.x, LookInput.y);
        CurrentPitch -= lookInput.y * (InvertY ? -1f : 1f);

        // X방향 회전은 플레이어 전체가 회전하고 Y방향 회전은 카메라만 회전하도록 설정합니다.
        transform.Rotate((InvertX ? -1f : 1f) * lookInput.x * Vector3.up);
        CameraManager.transform.localRotation = Quaternion.Euler(CurrentPitch, 0f, 0f);
        // 기울이기 및 반동을 적용합니다.
        transform.localRotation *= Quaternion.Euler(0f, ResultRecoilRotation.y, 0f);
        CameraManager.transform.localRotation *= Quaternion.Euler(ResultLeanRotation + new Vector3(ResultRecoilRotation.x, 0f, ResultRecoilRotation.z));
        CameraManager.transform.localPosition = CameraOffset + ResultLeanPosition;
    }

    // 카메라 반동 감소를 적용합니다.
    private void ApplyRecoilDamping()
    {
        CurrentRecoil = Vector3.Lerp(CurrentRecoil, Vector3.zero, Time.deltaTime * RecoilReturnSpeed);
        ResultRecoilRotation = Vector3.Slerp(ResultRecoilRotation, CurrentRecoil, Time.fixedDeltaTime * RecoilDampTime);
    }

    // 카메라 반동을 적용합니다.
    public void ApplyRecoil()
    {
        if (!UseCameraRecoil) return;

        // 반동은 임의로 X, Y, Z축에 적용됩니다.
        float recoilX = -1f * RecoilAmount.x;
        float recoilY = Random.Range(-RecoilAmount.y, RecoilAmount.y);
        float recoilZ = Random.Range(-RecoilAmount.z, RecoilAmount.z);

        CurrentRecoil += new Vector3(recoilX, recoilY, recoilZ);
    }

    #endregion

    #region Unity Methods

    public void Start()
    {
        if (PlayerCamera == null)
        {
            PlayerCamera = GetComponentInChildren<Camera>();
        }
        if (CharacterController == null)
        {
            CharacterController = GetComponent<CharacterController>();
        }
        if (CameraManager == null)
        {
            CameraManager = GetComponentInChildren<CameraManager>();
        }
        if (MeshManager == null)
        {
            MeshManager = GetComponentInChildren<MeshManager>();
        }

        // 마우스 커서 잠금 설정
        if (LockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // 카메라 초기 설정
        PlayerCamera.fieldOfView = CameraFOV;

        CameraManager.transform.localPosition = CameraOffset;
        MeshManager.transform.localPosition = -CameraOffset;
    }

    public void OnValidate()
    {
        if (PlayerCamera == null)
        {
            PlayerCamera = GetComponentInChildren<Camera>();
        }
        if (CharacterController == null)
        {
            CharacterController = GetComponent<CharacterController>();
        }
        if (CameraManager == null)
        {
            CameraManager = GetComponentInChildren<CameraManager>();
        }
        if (MeshManager == null)
        {
            MeshManager = GetComponentInChildren<MeshManager>();
        }
    }

    public void FixedUpdate()
    {
        if (UseCameraRecoil) ApplyRecoilDamping();
    }

    public void Update()
    {
        // 카메라 설정 업데이트
        CameraSettingsUpdate();
        // 카메라 기울이기
        LeanUpdate();
        // 카메라 업데이트
        LookUpdate();
    }

    #endregion
}
