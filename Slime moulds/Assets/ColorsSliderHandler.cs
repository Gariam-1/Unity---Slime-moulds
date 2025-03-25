using UnityEngine;
using UnityEngine.UI;

public class ColorsSliderHandler : MonoBehaviour
{
    private Slider colorsSlider;
    public Toggle colorsRandom;
    public Text colorsNumber;
    public int numColors = 2;
    public bool random = true;

    void Awake()
    {
        colorsSlider = GetComponent<Slider>();
        colorsSlider.value = PlayerPrefs.GetFloat("colorsSlider", 2f);
        colorsSlider.onValueChanged.AddListener(delegate { SettingsChanged(); });
        numColors = (int)Mathf.Min(Mathf.Exp(colorsSlider.value * 0.555f), 256f);
        colorsNumber.text = numColors.ToString();

        colorsRandom.isOn = PlayerPrefs.GetInt("colorsRandom", 1) == 1;
        colorsRandom.onValueChanged.AddListener(delegate { SettingsChanged(); });
        random = colorsRandom.isOn;
    }

    void SettingsChanged()
    {
        numColors = (int)Mathf.Min(Mathf.Exp(colorsSlider.value * 0.555f), 256f);
        colorsNumber.text = numColors.ToString();
        PlayerPrefs.SetFloat("colorsSlider", colorsSlider.value);
        random = colorsRandom.isOn;
        PlayerPrefs.SetInt("colorsRandom", random ? 1 : 0);
    }
}