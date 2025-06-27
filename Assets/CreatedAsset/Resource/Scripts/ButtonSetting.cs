using UnityEngine;
using UnityEngine.UI;

public class ButtonSetting : MonoBehaviour
{
    float alphaThreshold = 0.1f;
    private void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
    
}
