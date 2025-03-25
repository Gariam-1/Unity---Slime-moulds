using UnityEngine;
using UnityEngine.UI;

public class BloomToggleHandler : MonoBehaviour
{
    private Toggle bloomToggle;

    void Awake()
    {
        bloomToggle = GetComponent<Toggle>();
        bloomToggle.isOn = PlayerPrefs.GetInt("bloomToggle", 1) == 1;
        bloomToggle.onValueChanged.AddListener(delegate { SettingsChanged(); });
    }

    void SettingsChanged()
    {
        PlayerPrefs.SetInt("bloomToggle", bloomToggle.isOn ? 1 : 0);
    }
}