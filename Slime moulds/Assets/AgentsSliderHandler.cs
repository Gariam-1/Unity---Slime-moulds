using UnityEngine;
using UnityEngine.UI;

public class AgentsSliderHandler : MonoBehaviour
{
    private Slider agentsSlider;
    public Text agentsNumber;
    public int numAgents;

    void Awake()
    {
        agentsSlider = GetComponent<Slider>();
        agentsSlider.value = PlayerPrefs.GetFloat("agentsSlider", 1.0f);
        agentsSlider.onValueChanged.AddListener(delegate { SettingsChanged(); });
        numAgents = (int)Mathf.LerpUnclamped(1000f, Screen.width * Screen.height * 0.2f, agentsSlider.value);
        agentsNumber.text = numAgents.ToString();
    }

    void SettingsChanged()
    {
        numAgents = (int)Mathf.LerpUnclamped(1000f, Screen.width * Screen.height * 0.2f, agentsSlider.value);
        agentsNumber.text = numAgents.ToString();
        PlayerPrefs.SetFloat("agentsSlider", agentsSlider.value);
    }
}