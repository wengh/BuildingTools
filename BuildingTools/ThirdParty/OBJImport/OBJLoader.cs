﻿/*
(C) 2015 AARO4130
DO NOT USE PARTS OF, OR THE ENTIRE SCRIPT, AND CLAIM AS YOUR OWN WORK
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BrilliantSkies.Core.Logger;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class OBJLoader
{
    public static bool splitByMaterial = false;
    public static string[] searchPaths = new string[] {
        "",
        "%FileName%_Textures" + Path.DirectorySeparatorChar,
        "textures" + Path.DirectorySeparatorChar
    };
    //structures
    struct OBJFace
    {
        public string materialName;
        public string meshName;
        public int[] indexes;
    }


    //functions
#if UNITY_EDITOR
    [MenuItem("GameObject/Import From OBJ")]
    static void ObjLoadMenu()
    {
        string pth = UnityEditor.EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
        if (!string.IsNullOrEmpty(pth))
        {
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            LoadOBJFile(pth);
            AdvLogger.LogInfo("OBJ load took " + s.ElapsedMilliseconds + "ms");
            s.Stop();
        }
        }
#endif

    public static Vector3 ParseVectorFromCMPS(string[] cmps)
    {
        float x = float.Parse(cmps[1]);
        float y = float.Parse(cmps[2]);
        if (cmps.Length == 4)
        {
            float z = float.Parse(cmps[3]);
            return new Vector3(x, y, z);
        }
        return new Vector2(x, y);
    }
    public static Color ParseColorFromCMPS(string[] cmps, float scalar = 1.0f)
    {
        float Kr = float.Parse(cmps[1]) * scalar;
        float Kg = float.Parse(cmps[2]) * scalar;
        float Kb = float.Parse(cmps[3]) * scalar;
        return new Color(Kr, Kg, Kb);
    }

    public static string OBJGetFilePath(string path, string basePath, string fileName)
    {
        foreach (string sp in searchPaths)
        {
            string s = sp.Replace("%FileName%", fileName);
            if (File.Exists(basePath + s + path))
            {
                return basePath + s + path;
            }
            else if (File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
    public static Material[] LoadMTLFile(string fn, Shader shader)
    {
        Material mat = null;
        List<Material> matlList = new List<Material>();
        FileInfo mtlFileInfo = new FileInfo(fn);
        string baseFileName = Path.GetFileNameWithoutExtension(fn);
        string mtlFileDirectory = mtlFileInfo.Directory.FullName + Path.DirectorySeparatorChar;
        foreach (string ln in File.ReadAllLines(fn))
        {
            string l = ln.Trim().Replace("  ", " ");
            string[] cmps = l.Split(' ');
            string data = l.Remove(0, l.IndexOf(' ') + 1);

            bool isTransparent = shader.name.Contains("Transparent");
            bool isHologram = shader.name.Contains("Hologram");

            if (cmps[0] == "newmtl")
            {
                if (mat != null)
                {
                    matlList.Add(mat);
                }
                AdvLogger.LogInfo("[3D Holo] Assigning shader");
                mat = new Material(shader) { name = data };
                AdvLogger.LogInfo("[3D Holo] Done");
                mat.SetFloat("_RimPower", 10);
                mat.SetColor("_Color", new Color(1, 1, 1, 0.5f));
                mat.SetColor("_Emission", Color.white);
            }
            else if (cmps[0] == "Kd")
            {
                mat.SetColor("_MainColor", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "map_Kd")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                    mat.SetTexture("_MainTex", TextureLoader.LoadTexture(fpth));
            }
            else if (cmps[0] == "map_Bump")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                {
                    mat.SetTexture("_BumpMap", TextureLoader.LoadTexture(fpth, true));
                    mat.EnableKeyword("_NORMALMAP");
                }
            }
            else if (cmps[0] == "map_Ao")
            {
                //TEXTURE
                string fpth = OBJGetFilePath(data, mtlFileDirectory, baseFileName);
                if (fpth != null)
                {
                    mat.SetTexture("_OcclusionMap", TextureLoader.LoadTexture(fpth, true));
                    mat.EnableKeyword("_OCCLUSION");
                }
            }
            else if (cmps[0] == "Ks")
            {
                mat.SetColor("_RimColor", ParseColorFromCMPS(cmps));
                mat.SetColor("_SpecColor", ParseColorFromCMPS(cmps));
            }
            else if (cmps[0] == "Ns")
            {
                float Ns = float.Parse(cmps[1]);
                Ns = (Ns / 1000);
                mat.SetFloat("_Smooth", Ns);
            }
        }
        if (mat != null)
        {
            matlList.Add(mat);
        }
        return matlList.ToArray();
    }

    public static GameObject LoadOBJFile(string fn, Shader shader)
    {
        string meshName = Path.GetFileNameWithoutExtension(fn);

        bool hasNormals = false;
        //OBJ LISTS
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //UMESH LISTS
        List<Vector3> uvertices = new List<Vector3>();
        List<Vector3> unormals = new List<Vector3>();
        List<Vector2> uuvs = new List<Vector2>();
        //MESH CONSTRUCTION
        List<string> materialNames = new List<string>{""};
        List<string> objectNames = new List<string>();
        Dictionary<string, int> hashtable = new Dictionary<string, int>();
        List<OBJFace> faceList = new List<OBJFace>();
        string cmaterial = "";
        string cmesh = "default";
        //CACHE
        Material[] materialCache = null;
        //save this info for later
        FileInfo OBJFileInfo = new FileInfo(fn);

        foreach (string ln in File.ReadAllLines(fn))
        {
            if (ln.Length > 0 && ln[0] != '#')
            {
                string l = ln.Trim().Replace("  ", " ");
                string[] cmps = l.Split(' ');
                string data = l.Remove(0, l.IndexOf(' ') + 1);

                if (cmps[0] == "mtllib")
                {
                    //load cache
                    string pth = OBJGetFilePath(data, OBJFileInfo.Directory.FullName + Path.DirectorySeparatorChar, meshName);
                    if (pth != null)
                        materialCache = LoadMTLFile(pth, shader);

                }
                else if ((cmps[0] == "g" || cmps[0] == "o") && splitByMaterial == false)
                {
                    cmesh = data;
                    if (!objectNames.Contains(cmesh))
                    {
                        objectNames.Add(cmesh);
                    }
                }
                else if (cmps[0] == "usemtl")
                {
                    cmaterial = data;
                    if (!materialNames.Contains(cmaterial))
                    {
                        materialNames.Add(cmaterial);
                    }

                    if (splitByMaterial)
                    {
                        if (!objectNames.Contains(cmaterial))
                        {
                            objectNames.Add(cmaterial);
                        }
                    }
                }
                else if (cmps[0] == "v")
                {
                    //VERTEX
                    vertices.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "vn")
                {
                    //VERTEX NORMAL
                    normals.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "vt")
                {
                    //VERTEX UV
                    uvs.Add(ParseVectorFromCMPS(cmps));
                }
                else if (cmps[0] == "f")
                {
                    int[] indexes = new int[cmps.Length - 1];
                    for (int i = 1; i < cmps.Length; i++)
                    {
                        string felement = cmps[i];
                        int vertexIndex = -1;
                        int normalIndex = -1;
                        int uvIndex = -1;
                        if (felement.Contains("//"))
                        {
                            //doubleslash, no UVS.
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (felement.Count(x => x == '/') == 2)
                        {
                            //contains everything
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                            normalIndex = int.Parse(elementComps[2]) - 1;
                        }
                        else if (!felement.Contains("/"))
                        {
                            //just vertex inedx
                            vertexIndex = int.Parse(felement) - 1;
                        }
                        else
                        {
                            //vertex and uv
                            string[] elementComps = felement.Split('/');
                            vertexIndex = int.Parse(elementComps[0]) - 1;
                            uvIndex = int.Parse(elementComps[1]) - 1;
                        }
                        string hashEntry = vertexIndex + "|" + normalIndex + "|" + uvIndex;
                        if (hashtable.ContainsKey(hashEntry))
                        {
                            indexes[i - 1] = hashtable[hashEntry];
                        }
                        else
                        {
                            //create a new hash entry
                            indexes[i - 1] = hashtable.Count;
                            hashtable[hashEntry] = hashtable.Count;
                            uvertices.Add(vertices[vertexIndex]);
                            if (normalIndex < 0 || (normalIndex > (normals.Count - 1)))
                            {
                                unormals.Add(Vector3.zero);
                            }
                            else
                            {
                                hasNormals = true;
                                unormals.Add(normals[normalIndex]);
                            }
                            if (uvIndex < 0 || (uvIndex > (uvs.Count - 1)))
                            {
                                uuvs.Add(Vector2.zero);
                            }
                            else
                            {
                                uuvs.Add(uvs[uvIndex]);
                            }

                        }
                    }
                    if (indexes.Length < 5 && indexes.Length >= 3)
                    {
                        OBJFace f1 = new OBJFace
                        {
                            materialName = cmaterial,
                            indexes = new int[] { indexes[0], indexes[1], indexes[2] },
                            meshName = (splitByMaterial) ? cmaterial : cmesh
                        };
                        faceList.Add(f1);
                        if (indexes.Length > 3)
                        {

                            OBJFace f2 = new OBJFace
                            {
                                materialName = cmaterial,
                                meshName = (splitByMaterial) ? cmaterial : cmesh,
                                indexes = new int[] { indexes[2], indexes[3], indexes[0] }
                            };
                            faceList.Add(f2);
                        }
                    }
                }
            }
        }

        if (objectNames.Count == 0)
            objectNames.Add("default");

        //build objects
        GameObject parentObject = new GameObject(meshName);


        foreach (string obj in objectNames)
        {
            GameObject subObject = new GameObject(obj);
            subObject.transform.parent = parentObject.transform;
            subObject.transform.localScale = new Vector3(-1, 1, 1);
            //Create mesh
            Mesh m = new Mesh
            {
                name = obj,
                indexFormat = vertices.Count < ushort.MaxValue ? IndexFormat.UInt16 : IndexFormat.UInt32,
            };
            //LISTS FOR REORDERING
            List<Vector3> processedVertices = new List<Vector3>();
            List<Vector3> processedNormals = new List<Vector3>();
            List<Vector2> processedUVs = new List<Vector2>();
            List<int[]> processedIndexes = new List<int[]>();
            Dictionary<int, int> remapTable = new Dictionary<int, int>();
            //POPULATE MESH
            List<string> meshMaterialNames = new List<string>();

            OBJFace[] ofaces = faceList.Where(x => x.meshName == obj).ToArray();
            foreach (string mn in materialNames)
            {
                OBJFace[] faces = ofaces.Where(x => x.materialName == mn).ToArray();
                if (faces.Length > 0)
                {
                    meshMaterialNames.Add(mn);
                    if (m.subMeshCount != meshMaterialNames.Count)
                        m.subMeshCount = meshMaterialNames.Count;

                    int[] indexes = faces.SelectMany(x => x.indexes).ToArray();
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        int idx = indexes[i];
                        //build remap table
                        if (remapTable.ContainsKey(idx))
                        {
                            //ezpz
                            indexes[i] = remapTable[idx];
                        }
                        else
                        {
                            processedVertices.Add(uvertices[idx]);
                            processedNormals.Add(unormals[idx]);
                            processedUVs.Add(uuvs[idx]);
                            remapTable[idx] = processedVertices.Count - 1;
                            indexes[i] = remapTable[idx];
                        }
                    }

                    processedIndexes.Add(indexes);
                }
            }

            //apply stuff
            m.vertices = processedVertices.ToArray();
            m.normals = processedNormals.ToArray();
            m.uv = processedUVs.ToArray();

            for (int i = 0; i < processedIndexes.Count; i++)
            {
                m.SetTriangles(processedIndexes[i], i);
            }

            if (!hasNormals)
            {
                m.RecalculateNormals();
            }
            m.RecalculateBounds();

            MeshFilter mf = subObject.AddComponent<MeshFilter>();
            MeshRenderer mr = subObject.AddComponent<MeshRenderer>();

            Material[] processedMaterials = new Material[meshMaterialNames.Count];
            for (int i = 0; i < meshMaterialNames.Count; i++)
            {

                if (materialCache == null)
                {
                    processedMaterials[i] = new Material(Shader.Find("Standard"));
                }
                else
                {
                    Material mfn = Array.Find(materialCache, x => x.name == meshMaterialNames[i]); ;
                    if (mfn == null)
                    {
                        processedMaterials[i] = new Material(Shader.Find("Standard"));
                    }
                    else
                    {
                        processedMaterials[i] = mfn;
                    }

                }
                processedMaterials[i].name = meshMaterialNames[i];
            }

            mr.materials = processedMaterials;
            mf.mesh = m;

        }

        return parentObject;
    }
}