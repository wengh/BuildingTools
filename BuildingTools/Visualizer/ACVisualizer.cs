using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.CSharp;
using BrilliantSkies.Core.Types;
using BrilliantSkies.Ftd.Avatar;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildingTools.Visualizer
{
    public class ACVisualizer : MonoBehaviour
    {
        public ComputeShader visualizer;
        public AllConstruct c;
        public ComputeBuffer armor;
        public ComputeBuffer armorMultiplier;

        private RenderTexture target;
        private Camera camera;
        private int currentSample = 0;
        private Material addMaterial;

        private int kernel;

        private int cullingMask;
        private CameraClearFlags clearFlags;
        private List<Component> alreadyDisabledComponents = new List<Component>();

        public static float[] GetArmorFromConstruct(AllConstruct construct, out Vector3i shape)
        {
            var min = construct.iSize.GetMin();
            var max = construct.iSize.GetMax();

            print(min);
            print(max);

            var size = max - min + 1;
            print(size);

            float[,,] data = new float[size.x, size.y, size.z];

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
                        var item = construct.iBlocks[x + min.x, y + min.y, z + min.z]?.item;
                        if (item != null && item.ExtraSettings.StructuralComponent)
                        {
                            data[x, y, z] = item.ArmourClass;

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

            shape = new Vector3i(x2 - x1, y2 - y1, z2 - z1);
            print(shape);

            float[] flattened = new float[shape.x * shape.y * shape.z];
            for (int x = x1; x < x2; x++)
                for (int y = y1; y < y2; y++)
                    for (int z = z1; z < z2; z++)
                        flattened[x - x1 + shape.x * (y - y1 + shape.y * (z - z1))] = data[x, y, z];
            
            return flattened;
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
                if (c != this)
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
            transform.parent = null;
            c = ClientInterface.GetInterface().Get_I_All_ConstructableSelector().Get_LookSC_LookC_CloseCRay_CloseC();
            visualizer = BuildingToolsPlugin.bundle.LoadAllAssets<ComputeShader>()[0];
            
            camera = gameObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            cullingMask = camera.cullingMask;
            clearFlags = camera.clearFlags;
            camera.cullingMask = 0;
            camera.clearFlags = CameraClearFlags.Nothing;

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
            armor.Release();
            armorMultiplier.Release();
            camera.cullingMask = cullingMask;
            camera.clearFlags = clearFlags;
            SceneManager.LoadScene(0);
            //Destroy(GetComponent<FlyCamera>());
            //EnableAllScripts();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            UpdateShaderParameters();
            Render(destination);
        }

        private void UpdateShaderParameters()
        {
            visualizer.SetMatrix("CameraToWorld", camera.cameraToWorldMatrix);
            visualizer.SetMatrix("CameraInverseProjection", camera.projectionMatrix.inverse);
            visualizer.SetVector("PixelOffset", new Vector2(0.5f, 0.5f)/*new Vector2(UnityEngine.Random.value, UnityEngine.Random.value)*/);
        }

        private void InitializeShaderParameters()
        {
            float[] data = GetArmorFromConstruct(c, out var shape);
            armor = new ComputeBuffer(data.Length, 4);
            armor.SetData(data);

            armorMultiplier = new ComputeBuffer(8, 4);
            armorMultiplier.SetData(Block.AcContributionsPerLayer);

            kernel = visualizer.FindKernel("CSMain");

            visualizer.SetInts("Shape", new[] { shape.x, shape.y, shape.z });

            visualizer.SetBuffer(kernel, "ArmorMultiplier", armorMultiplier);
            visualizer.SetBuffer(kernel, "Armor", armor);
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
            Graphics.Blit(target, destination/*, addMaterial*/);
            currentSample++;
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
                currentSample = 0;
                transform.hasChanged = false;
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                Destroy(this);
            }
        }
    }
}
