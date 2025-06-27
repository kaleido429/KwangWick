using UnityEngine;
using UnityEngine.UI;

public class ScreenSettings : MonoBehaviour
{
    public Toggle fullscreenToggle;

    public void SetFullscreen(bool isOn)
    {
        Screen.fullScreen = isOn;
    }

    void Start()
    {
        fullscreenToggle.isOn = Screen.fullScreen;

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }
}
