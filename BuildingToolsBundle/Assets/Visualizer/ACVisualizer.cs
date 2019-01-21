using UnityEngine;

public class ACVisualizer : MonoBehaviour
{
    public ComputeShader visualizer;
    public ComputeBuffer armor;
    public ComputeBuffer armorMultiplier;

    private RenderTexture _target;
    private Camera _camera;
    private int currentSample = 0;
    private Material addMaterial;

    public static float[] AcContributionsPerLayer = new float[]
    {
        1f,
        0.85f,
        0.7f,
        0.55f,
        0.4f,
        0.25f,
        0.1f,
        0.1f
    };

    private int kernel;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        armor = new ComputeBuffer(200 * 50 * 30, 4);
        float[] data = new float[200 * 50 * 30];
        for (int i = 0; i < data.Length; i++)
        {
            int z = i / (200 * 50);
            int i2 = i - (z * 200 * 50);
            int y = i2 / 200;
            int x = i2 % 200;

            if (x < y * 4) continue;
            if (y > z * 3 / 2) continue;
            data[i] = Random.Range(0.1f, 20);
        }
        armor.SetData(data);

        armorMultiplier = new ComputeBuffer(8, 4);
        armorMultiplier.SetData(AcContributionsPerLayer);

        kernel = visualizer.FindKernel("CSMain");
        InitializeShaderParameters();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        UpdateShaderParameters();
        Render(destination);
    }

    private void UpdateShaderParameters()
    {
        visualizer.SetMatrix("CameraToWorld", _camera.cameraToWorldMatrix);
        visualizer.SetMatrix("CameraInverseProjection", _camera.projectionMatrix.inverse);
        visualizer.SetVector("PixelOffset", new Vector2(Random.value, Random.value));
    }

    private void InitializeShaderParameters()
    {
        visualizer.SetInts("Shape", new[] { 200, 50, 30 });

        visualizer.SetBuffer(kernel, "ArmorMultiplier", armorMultiplier);
        visualizer.SetBuffer(kernel, "Armor", armor);
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the target and dispatch the compute shader
        visualizer.SetTexture(kernel, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        visualizer.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        if (addMaterial == null)
            addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        addMaterial.SetFloat("_Weight", 1f / (currentSample + 1));
        Graphics.Blit(_target, destination, addMaterial);
        currentSample++;
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
            {
                enableRandomWrite = true
            };
            _target.Create();
        }
    }

    private void Update()
    {
        if (transform.hasChanged)
        {
            currentSample = 0;
            transform.hasChanged = false;
        }
    }
}