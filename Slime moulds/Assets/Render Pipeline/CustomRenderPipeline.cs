using UnityEngine;
using UnityEngine.Rendering;
using Kino;

public class CustomRenderPipeline : RenderPipeline
{
    readonly RenderTexture renderTexture;
    readonly Main computeShader;
    readonly Bloom bloomShader;
    private bool HDR;

    public CustomRenderPipeline(){        
        HDR = HDROutputSettings.main.available && HDROutputSettings.main.active;
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB2101010);
        
        Camera camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        camera.allowHDR = true;

        computeShader = camera.GetComponent<Main>();
        bloomShader = camera.GetComponent<Bloom>();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras){
        if (cameras.Length == 1){
            // Switch on HDR if available
            if (HDR != HDROutputSettings.main.active){
                HDR = HDROutputSettings.main.available && HDROutputSettings.main.active;
                HDROutputSettings.main.RequestHDRModeChange(HDR);
                HDROutputSettings.main.automaticHDRTonemapping = !HDR;
            }

            // Set up camera rendering
            context.SetupCameraProperties(cameras[0]);

            // Compute shader
            computeShader.OnRenderImage(renderTexture, renderTexture);

            // Post-processing shader
            if (bloomShader.enabled) bloomShader.OnRenderImage(renderTexture, cameras[0].targetTexture);
            else Graphics.Blit(renderTexture, cameras[0].targetTexture);

            // Draw UI
            context.DrawUIOverlay(cameras[0]);

            context.Submit();
        }
    }
}