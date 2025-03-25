using UnityEngine;
using UnityEngine.UI;

public class CleanRestartToggleHandler : MonoBehaviour
{
    private Toggle cleanRestartToggle;
    public bool clean = false;

    void Awake()
    {
        cleanRestartToggle = GetComponent<Toggle>();
        cleanRestartToggle.isOn = PlayerPrefs.GetInt("cleanRestartToggle", 0) == 1;
        cleanRestartToggle.onValueChanged.AddListener(delegate { SettingsChanged(); });
        clean = cleanRestartToggle.isOn;
    }

    void SettingsChanged()
    {
        clean = cleanRestartToggle.isOn;
        PlayerPrefs.SetInt("cleanRestartToggle", clean ? 1 : 0);
    }
}