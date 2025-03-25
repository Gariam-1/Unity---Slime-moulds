using UnityEngine;
using UnityEngine.UI;

public class FpsSliderHandler : MonoBehaviour
{
    private Slider fpsSlider;
    public Text fpsNumber;

    void Awake()
    {
        fpsSlider = GetComponent<Slider>();
        fpsSlider.maxValue = (float)Screen.currentResolution.refreshRateRatio.value;
        fpsSlider.value = (float)PlayerPrefs.GetInt("fpsSlider", Mathf.Min(60, (int)Screen.currentResolution.refreshRateRatio.value));
        Application.targetFrameRate = (int)fpsSlider.value;
        fpsNumber.text = ((int)fpsSlider.value).ToString();
        fpsSlider.onValueChanged.AddListener(delegate { SettingsChanged(); });
    }

    void SettingsChanged()
    {
        Application.targetFrameRate = (int)fpsSlider.value;
        fpsNumber.text = ((int)fpsSlider.value).ToString();
        PlayerPrefs.SetInt("fpsSlider", Application.targetFrameRate);
    }
}