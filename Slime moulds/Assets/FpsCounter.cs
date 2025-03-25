using UnityEngine;
using UnityEngine.UI;

public class FpsCounter : MonoBehaviour
{
    private Text textComponent;
    private int frames = 1;
    private float fromLastFrame;

    void Start()
    {
        textComponent = GetComponent<Text>();
        fromLastFrame = 1.0f / Application.targetFrameRate;
        InvokeRepeating("updateFrameRate", 0f, 0.5f);
        textComponent.text = $"FPS: {Mathf.Round(frames / Time.deltaTime)}";
    }

    void Update()
    {
        frames++;
        fromLastFrame += Time.deltaTime;
    }

    void updateFrameRate()
    {
        textComponent.text = $"FPS: {Mathf.Round(frames / fromLastFrame)}";
        frames = 0;
        fromLastFrame = 0.0f;
    }
}