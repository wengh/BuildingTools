using System.Collections.Generic;
using BrilliantSkies.Core.SteamworksIntegration;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Avatar;
using BrilliantSkies.PlayerProfiles;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildingTools.Visualizer
{
    public class ACVisualizer : MonoBehaviour
    {
        public ComputeShader visualizer;
        public AllConstruct c;

        public ComputeBuffer id;
        public ComputeBuffer armor;
        public ComputeBuffer health;

        public ComputeBuffer armorMultiplier;

        public float maxStrength = 1000000;
        public int maxAirgaps = 5;

        private RenderTexture target;
        private Camera camera;
        // private int currentSample = 0;
        // private Material addMaterial;

        private int kernel;

        private List<Component> alreadyDisabledComponents = new List<Component>();

        public Vector3i SetBlockDataFromConstruct(AllConstruct construct)
        {
            var min = construct.iSize.GetMin();
            var max = construct.iSize.GetMax();

            print(min);
            print(max);

            var size = max - min + 1;
            print(size);

            int[,,] idData = new int[size.x, size.y, size.z];
            float[,,] armorData = new float[size.x, size.y, size.z];
            float[,,] healthData = new float[size.x, size.y, size.z];

            int x1 = size.x;
            int y1 = size.y;
            int z1 = size.z;

            int x2 = 0;
            int y2 = 0;
            int z2 = 0;

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        var block = construct.iBlocks[x + min.x, y + min.y, z + min.z];
                        idData[x, y, z] = block?.GetHashCode() ?? -1;
                        if (block != null)
                        {
                            armorData[x, y, z] = block.item.ExtraSettings.StructuralComponent
                                ? block.item.ArmourClass
                                : -block.item.ArmourClass;
                            healthData[x, y, z] = block.item.Health;

                            if (x < x1) x1 = x;
                            if (y < y1) y1 = y;
                            if (z < z1) z1 = z;

                            if (x > x2) x2 = x;
                            if (y > y2) y2 = y;
                            if (z > z2) z2 = z;
                        }
                    }
                }
            };

            var shape = new Vector3i(x2 - x1, y2 - y1, z2 - z1);
            print(shape);

            int[] idFlattened = new int[shape.x * shape.y * shape.z];
            float[] armorFlattened = new float[shape.x * shape.y * shape.z];
            float[] healthFlattened = new float[shape.x * shape.y * shape.z];

            for (int x = x1; x < x2; x++)
                for (int y = y1; y < y2; y++)
                    for (int z = z1; z < z2; z++)
                    {
                        idFlattened[x - x1 + shape.x * (y - y1 + shape.y * (z - z1))] = idData[x, y, z];
                        armorFlattened[x - x1 + shape.x * (y - y1 + shape.y * (z - z1))] = armorData[x, y, z];
                        healthFlattened[x - x1 + shape.x * (y - y1 + shape.y * (z - z1))] = healthData[x, y, z];
                    }

            id = new ComputeBuffer(idFlattened.Length, 4);
            id.SetData(idFlattened);

            armor = new ComputeBuffer(armorFlattened.Length, 4);
            armor.SetData(armorFlattened);

            health = new ComputeBuffer(healthFlattened.Length, 4);
            health.SetData(healthFlattened);

            return shape;
        }

        public static T[][][] Create3DArray<T>(int x, int y, int z)
        {
            T[][][] result = new T[x][][];
            for (int i = 0; i < x; i++)
            {
                var tmp = result[i] = new T[y][];
                for (int j = 0; j < y; j++)
                    tmp[j] = new T[z];
            }
            return result;
        }

        private void DisableAllScripts()
        {
            foreach (var c in Resources.FindObjectsOfTypeAll<MonoBehaviour>())
            {
                if (c.transform != transform)
                {
                    if (c.enabled)
                        c.enabled = false;
                    else
                        alreadyDisabledComponents.Add(c);
                }
            }
            Time.timeScale = 0;
        }

        private void EnableAllScripts()
        {
            foreach (var c in Resources.FindObjectsOfTypeAll<Behaviour>())
            {
                if (!alreadyDisabledComponents.Contains(c) && c.transform != transform && !transform.IsChildOf(c.transform))
                    c.enabled = true;
            }
            Time.timeScale = 1;
        }

        private void Awake()
        {
            ProfileManager.Instance.SaveAll();

            transform.parent = null;
            c = ClientInterface.GetInterface().Get_I_All_ConstructableSelector().Get_LookSC_LookC_CloseCRay_CloseC();
            visualizer = BuildingToolsPlugin.bundle.LoadAllAssets<ComputeShader>()[0];

            camera = gameObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.Nothing;
            Cursor.lockState = CursorLockMode.Locked;

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;

            InitializeShaderParameters();

            //DisableAllScripts();
            gameObject.AddComponent<FlyCamera>();
            //transform.position = new Vector3(0, -50, 0);

            foreach (var i in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (i != gameObject)
                    Destroy(i);
            }
        }

        private void OnDestroy()
        {
            //Destroy(gameObject);
            //EnableAllScripts();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            UpdateShaderParameters();
            Render(destination);
            //print("Rendering");
        }

        private void UpdateShaderParameters()
        {
            visualizer.SetMatrix("CameraToWorld", camera.cameraToWorldMatrix);
            visualizer.SetMatrix("CameraInverseProjection", camera.projectionMatrix.inverse);
            //visualizer.SetVector("PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
            visualizer.SetVector("PixelOffset", new Vector2(0.5f, 0.5f));
        }

        private void InitializeShaderParameters()
        {
            var shape = SetBlockDataFromConstruct(c);

            armorMultiplier = new ComputeBuffer(Block.AcContributionsPerLayer.Length, 4);
            armorMultiplier.SetData(Block.AcContributionsPerLayer);

            kernel = visualizer.FindKernel("CSMain");

            visualizer.SetInts("Shape", new[] { shape.x, shape.y, shape.z });

            visualizer.SetBuffer(kernel, "ArmorMultiplier", armorMultiplier);
            visualizer.SetBuffer(kernel, "Id", id);
            visualizer.SetBuffer(kernel, "Armor", armor);
            visualizer.SetBuffer(kernel, "Health", health);

            visualizer.SetInt("ArmorMultiplierLastIndex", armorMultiplier.count - 1);
            visualizer.SetFloat("MaxStrength", maxStrength);
            visualizer.SetInt("MaxAirgaps", maxAirgaps);
        }

        private void Render(RenderTexture destination)
        {
            // Make sure we have a current render target
            InitRenderTexture();

            // Set the target and dispatch the compute shader
            visualizer.SetTexture(kernel, "Result", target);
            int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
            visualizer.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);

            // Blit the result texture to the screen
            // if (addMaterial == null)
            //     addMaterial = new Material(BuildingToolsPlugin.bundle.LoadAllAssets<Shader>().First(x => x.name.Contains("AddShader")));
            // addMaterial.SetFloat("_Weight", 1f / (currentSample + 1));
            // Graphics.Blit(target, destination, addMaterial);
            // currentSample++;
            Graphics.Blit(target, destination);
        }

        private void InitRenderTexture()
        {
            if (target == null || target.width != Screen.width || target.height != Screen.height)
            {
                // Release render texture if we already have one
                if (target != null)
                    target.Release();

                // Get a render target for Ray Tracing
                target = new RenderTexture(Screen.width, Screen.height, 0,
                    RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
                {
                    enableRandomWrite = true
                };
                target.Create();
            }
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                // currentSample = 0;
                transform.hasChanged = false;
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                id.Release();
                armor.Release();
                health.Release();
                armorMultiplier.Release();
                Resources.UnloadAsset(visualizer);

                new SteamInterface().__RestartGame();
                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                //Application.Quit();
                //Destroy(this);
            }
        }
    }
}
