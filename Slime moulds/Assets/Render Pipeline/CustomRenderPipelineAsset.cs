using UnityEngine;
using UnityEngine.Rendering;
    
[CreateAssetMenu(menuName = "Rendering/CustomRenderPipelineAsset")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline() {
        if (HDROutputSettings.main.available) {
            HDROutputSettings.main.RequestHDRModeChange(true);
            HDROutputSettings.main.automaticHDRTonemapping = false;
        }
        return new CustomRenderPipeline();
    }
}