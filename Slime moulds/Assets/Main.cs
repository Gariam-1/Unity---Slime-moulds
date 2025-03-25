using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

public struct Agent{
    public Vector2 position;
    public float angle;
    public float colorIndex;
}

public class Main : MonoBehaviour
{
    public ComputeShader computeshader;
    public AgentsSliderHandler agentsSlider;
    public ColorsSliderHandler colorsSlider;
    public CleanRestartToggleHandler cleanRestartToggle;
    private RenderTexture trailTexture;
    private RenderTexture dissolveTexture;
    public Vector2 start = new(0.5f, 0.5f);
    public Vector2 speed = new(25.0f, 150.0f);
    public Vector2 dissolveSpeed = new(0f, 0.3f);
    public Vector2 diffuseRate = new(1.0f, 30.0f);
    public Vector2 sensorAngle = new(30.0f, 60.0f);
    public Vector2 sensorDist = new(10.0f, 150.0f);
    public float sensorSize = 1.0f;
    public Vector2 turnRate = new(0.1f, 150.0f);
    public Vector2 turnRandom = new(0.0f, 0.5f);
    private Agent[] agents;
    private ComputeBuffer agentsBufferWrite, agentsBufferRead;
    private int kernelCompute, kernelDissolveX, kernelDissolveY, computeThreadGroups;
    private uint computeThreadGroupSize;
    private readonly float[] timeOffsets = new float[7];
    private readonly float[] speedMultipliers = new float[7];
    private const float PI = 3.1415926536f, DEG2RAD = 0.01745329f, RAD2DEG = 57.29578f;
    private float screenSizeFactor;
    private Vector2Int dissolveThreadGroups;
    private NativeArray<Agent> tempArray;
    private readonly Vector4[] colorArray = new Vector4[256];

    void Awake(){
        screenSizeFactor = Screen.height / 1440.0f;

        // Find shaders
        kernelCompute = computeshader.FindKernel("CSMain");
        kernelDissolveX = computeshader.FindKernel("DissolveX");
        kernelDissolveY = computeshader.FindKernel("DissolveY");

        // Create render textures
        trailTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB2101010){enableRandomWrite = true};
        trailTexture.Create();
        dissolveTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB2101010){enableRandomWrite = true};
        dissolveTexture.Create();

        // Assign textures to the shaders
        computeshader.SetTexture(kernelCompute, "TrailMap", trailTexture);
        computeshader.SetTexture(kernelDissolveX, "TrailMap", trailTexture);
        computeshader.SetTexture(kernelDissolveX, "DissolveMap", dissolveTexture);
        computeshader.SetTexture(kernelDissolveY, "TrailMap", trailTexture);
        computeshader.SetTexture(kernelDissolveY, "DissolveMap", dissolveTexture);

        // Set shader constants
        computeshader.SetInts("resolution", new int[] {Screen.width, Screen.height});

        // Get shader thread sizes
        computeshader.GetKernelThreadGroupSizes(kernelCompute, out computeThreadGroupSize, out uint y, out uint z);
        computeshader.GetKernelThreadGroupSizes(kernelDissolveX, out uint dissolveThreadGroupSize, out y, out z);
        dissolveThreadGroups = new(Screen.width / (int)dissolveThreadGroupSize, Screen.height / (int)dissolveThreadGroupSize);

        CreateAgents();
    }

    float TriangleSin01(float v){
        return 2f * Mathf.Abs(v - Mathf.Floor(v + 0.75f) + 0.25f);
    }

    float MouseDown(){
        bool mouse0Down = Input.GetMouseButton(0);
        if ((mouse0Down || Input.GetMouseButton(2)) && !EventSystem.current.IsPointerOverGameObject()) return mouse0Down ? 1f : -1f;
        return 0f;
    }

    void SendProperties(){
        computeshader.SetFloats("hueShift", Time.time * 0.015f);
        computeshader.SetFloat("randomSeed", Random.value);
        float currentSpeed = screenSizeFactor * Mathf.Lerp(speed.x, speed.y, TriangleSin01(Time.time * speedMultipliers[0] + timeOffsets[0]));
        //float speedFactor = currentSpeed / 100f;
        float currentDissolveSpeed = Mathf.Lerp(dissolveSpeed.x, dissolveSpeed.y, TriangleSin01(Time.time * speedMultipliers[1] + timeOffsets[1]));
        float currentDiffuseRate = Mathf.Lerp(diffuseRate.x, diffuseRate.y, TriangleSin01(Time.time * speedMultipliers[2] + timeOffsets[2]));
        computeshader.SetFloats("speed", new float[] { currentSpeed, currentDissolveSpeed, currentDiffuseRate });

        float currentSensorAngle = DEG2RAD * Mathf.Lerp(sensorAngle.x, sensorAngle.y, TriangleSin01(Time.time * speedMultipliers[3] + timeOffsets[3]));
        float currentSensorDist = screenSizeFactor * Mathf.Lerp(sensorDist.x, sensorDist.y, TriangleSin01(Time.time * speedMultipliers[4] + timeOffsets[4]));
        computeshader.SetFloats("sensor", new float[] { currentSensorAngle, currentSensorDist, sensorSize });

        float currentTurnRate = PI * Mathf.Lerp(turnRate.x, turnRate.y, Mathf.Exp(-TriangleSin01(Time.time * speedMultipliers[5] + timeOffsets[5]) * 3f));
        float currentTurnRandom = Mathf.Lerp(turnRandom.x, turnRandom.y, TriangleSin01(Time.time * speedMultipliers[6] + timeOffsets[6]));
        computeshader.SetFloats("turn", new float[] { currentTurnRate, currentTurnRandom });
    }

    void FixedUpdate(){
        if (Time.deltaTime <= Time.fixedDeltaTime) SendProperties();
    }

    void Update(){
        if (Time.deltaTime > Time.fixedDeltaTime) SendProperties();

        computeshader.SetFloats("time", new float[] {Time.time, Time.deltaTime});
        computeshader.SetFloats("mouse", new float[] {Input.mousePosition.x, Input.mousePosition.y, MouseDown()});
        
        // Prepare agents array to be sent to the shader (less memcpy, thus faster, than SetData)
        tempArray = agentsBufferRead.BeginWrite<Agent>(0, agents.Length);
        NativeArray<Agent>.Copy(agents, tempArray);
        agentsBufferRead.EndWrite<Agent>(agents.Length);
        
        // Execute the shader
        computeshader.Dispatch(kernelCompute, computeThreadGroups, 1, 1);

        // Retrieve updated agents array from the shader
        agentsBufferWrite.GetData(agents);

        // Execute shader to blur the image thus dissolving the trails
        computeshader.Dispatch(kernelDissolveX, dissolveThreadGroups.x, dissolveThreadGroups.y, 1);
        computeshader.Dispatch(kernelDissolveY, dissolveThreadGroups.x, dissolveThreadGroups.y, 1);
        Graphics.Blit(dissolveTexture, trailTexture);
    }

    public void OnRenderImage(RenderTexture src, RenderTexture dest){
        Graphics.Blit(trailTexture, dest);
    }

    Vector3 Hue(Vector3 color, float hueShift){
	    Vector3 k = new(0.57735f, 0.57735f, 0.57735f);
	    float cosAngle = Mathf.Cos(hueShift);
        Vector3 newColor = Vector3.Cross(k, color) * Mathf.Sin(hueShift) + (1.0f - cosAngle) * Vector3.Dot(k, color) * k;
	    return color * cosAngle + newColor;
    }

    public void ClearTextures(){
        RenderTexture activeRenderTexture = RenderTexture.active;

        RenderTexture.active = trailTexture;
        GL.Clear(true, true, Color.black);

        RenderTexture.active = dissolveTexture;
        GL.Clear(true, true, Color.black);

        RenderTexture.active = activeRenderTexture;
    }

    public void CreateAgents(){
        agentsBufferWrite?.Dispose();
        agentsBufferRead?.Dispose();

        float numColors;
        if (colorsSlider.random) numColors = Mathf.Floor(Random.Range(1f, colorsSlider.numColors));
        else numColors = colorsSlider.numColors;

        int i;
        for (i = 0; i < timeOffsets.Length; i++) {
            timeOffsets[i] = Random.Range(-2f, 2f);
            speedMultipliers[i] = Random.Range(0f, 0.01f) * Mathf.Sign(timeOffsets[i]);
        }

        float colorSeed = Random.value * 180f;
        float angleShift = Random.value;
        agents = new Agent[agentsSlider.numAgents];

        for (i = 0; i < agentsSlider.numAgents; i++){
            agents[i] = new Agent(){
                position = new(Screen.width * start.x, Screen.height * start.y),
                angle = i / (float)agentsSlider.numAgents
            };
            
            agents[i].colorIndex = Mathf.Floor(agents[i].angle * numColors);
            colorArray[(int)agents[i].colorIndex] = (Vector4)Hue(new Vector3(1f, 0f, 0f), agents[i].colorIndex + colorSeed);
            agents[i].angle = (agents[i].angle + angleShift) * (PI + PI);
        }

        computeshader.SetInt("numAgents", agents.Length);
        computeshader.SetVectorArray("colors", colorArray);

        computeThreadGroups = agents.Length / (int)computeThreadGroupSize;
        int structSize = Marshal.SizeOf(typeof(Agent));
        agentsBufferRead = new ComputeBuffer(agents.Length, structSize, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        computeshader.SetBuffer(kernelCompute, "agentsRead", agentsBufferRead);
        agentsBufferWrite = new ComputeBuffer(agents.Length, structSize);
        computeshader.SetBuffer(kernelCompute, "agentsWrite", agentsBufferWrite);
    }

    public void Restart(){
        if (cleanRestartToggle.clean) ClearTextures();
        CreateAgents();
    }
}