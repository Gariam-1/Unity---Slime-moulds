using UnityEngine;

public class SettingsTransforms : MonoBehaviour
{
    public RectTransform fpsCounter;
    public RectTransform restartButton;
    public RectTransform settingsButton;
    public RectTransform settingsMenu;
    public Vector2 fpsCounterPosition = new(1f, 1f);
    public Vector2 restartButtonPosition = new(0f, 1f);
    public Vector2 settingsButtonPosition = new(1f, 1f);
    public Vector2 menuPosition = new(1f, 1f);

    void Start()
    {
        Place();
        GameObject.Find("Menu").SetActive(false);
    }

    void Place()
    {
        fpsCounter.position = new Vector3(Screen.width * fpsCounterPosition.x, Screen.height * fpsCounterPosition.y);
        restartButton.position = new Vector3(Screen.width * restartButtonPosition.x, Screen.height * restartButtonPosition.y);
        settingsButton.position = new Vector3(Screen.width * settingsButtonPosition.x, Screen.height * settingsButtonPosition.y);
        settingsMenu.position = new Vector3(Screen.width * menuPosition.x, Screen.height * menuPosition.y);

        float scale = (float)Screen.height / 1440.0f;
        fpsCounter.localScale = new Vector3(scale, scale, 1.0f);
        restartButton.localScale = new Vector3(scale, scale, 1.0f);
        settingsButton.localScale = new Vector3(scale, scale, 1.0f);
        settingsMenu.localScale = new Vector3(scale, scale, 1.0f);
    }

    public void ChangePosition()
    {
        fpsCounterPosition.x = 1f - fpsCounterPosition.x;
        restartButtonPosition.x = 1f - restartButtonPosition.x;
        settingsButtonPosition.x = 1f - settingsButtonPosition.x;
        menuPosition.x = 1f - menuPosition.x;
        
        settingsMenu.pivot = new Vector2(Mathf.Max(0f, menuPosition.x), 1f);
        settingsButton.pivot = new Vector2(Mathf.Max(0f, settingsButtonPosition.x), 1f);
        restartButton.pivot = new Vector2(Mathf.Max(0f, restartButtonPosition.x), 1f);
        fpsCounter.pivot = new Vector2(Mathf.Max(0f, fpsCounterPosition.x), 1f);

        Place();
    }
}
