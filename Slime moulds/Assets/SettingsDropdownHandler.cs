using UnityEngine;
using UnityEngine.UI;

public class SettingsDropdownHandler : MonoBehaviour
{
    private Dropdown settingsPositionDropdown;
    public bool isRight = true;

    void Awake()
    {
        settingsPositionDropdown = GetComponent<Dropdown>();
        settingsPositionDropdown.value = PlayerPrefs.GetInt("settingsPositionDropdown", 0) == 0 ? 0 : 1;
        settingsPositionDropdown.onValueChanged.AddListener(delegate { SettingsChanged(); });
        isRight = settingsPositionDropdown.value == 0;
    }

    void SettingsChanged()
    {
        isRight = settingsPositionDropdown.value == 0;
        PlayerPrefs.SetInt("settingsPositionDropdown", isRight ? 0 : 1);
    }
}