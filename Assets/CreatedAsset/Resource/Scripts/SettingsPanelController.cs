using UnityEngine;

public class SettingsPanelController : MonoBehaviour
{
    public GameObject SettingsPanel;

    private void Start()
    {
        // 초기에는 비활성화
        SettingsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        SettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
    }
}
