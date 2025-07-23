using UnityEngine;

public class FrameCounter : MonoBehaviour
{
    private float deltaTime = 0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, Screen.width, Screen.height);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 50;
        style.normal.textColor = Color.white;

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS ({1:0.0} ms)", fps, msec);

        GUI.Label(rect, text, style);
    }
}
