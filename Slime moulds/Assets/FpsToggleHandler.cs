using UnityEngine;
using UnityEngine.UI;

public class FpsToggleHandler : MonoBehaviour
{
    private Toggle fpsToggle;

    void Awake()
    {
        fpsToggle = GetComponent<Toggle>();
        fpsToggle.isOn = PlayerPrefs.GetInt("fpsToggle", 1) == 1;
        fpsToggle.onValueChanged.AddListener(delegate { SettingsChanged(); });
    }

    void SettingsChanged()
    {
        PlayerPrefs.SetInt("fpsToggle", fpsToggle.isOn ? 1 : 0);
    }
}