using UnityEngine;
using System.Runtime.InteropServices;

public class WebGLResizer : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SetCanvasSizeByHeight();
#endif

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("Calling JS...");
        Debug.Log("Calling JS: SetCanvasSizeByHeight");
        SetCanvasSizeByHeight();
#endif
    }
}
