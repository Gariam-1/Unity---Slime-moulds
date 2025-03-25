using UnityEngine;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour
{
    Text textComponent;
    
    void OnEnable(){
        textComponent = GetComponent<Text>();
    }

    void FixedUpdate(){
        textComponent.text = $"Color gamut: {Graphics.activeColorGamut}";
    }
}
