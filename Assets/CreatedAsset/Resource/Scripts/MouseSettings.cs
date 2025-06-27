using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSettings : MonoBehaviour
{
    public Slider sensitivitySlider;
    public TMP_Text sensitivityText;

    public static float sensitivity = 1.0f;

    void Start()
    {
        sensitivitySlider.value = sensitivity;
        UpdateText(sensitivity);

        sensitivitySlider.onValueChanged.AddListener((value) =>
        {
            sensitivity = value;
            UpdateText(value);
        });
    }

    void UpdateText(float value)
    {
        sensitivityText.text = $"{value:F2}";
    }
}
