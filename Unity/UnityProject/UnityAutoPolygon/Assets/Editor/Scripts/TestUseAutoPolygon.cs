using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace App.Utils
{
    public static class TestUseAutoPolygon
    {
        [MenuItem("YzyTools/test TestUseAutoPolygon #1 ")]
        static void DoTest()
        {
            try
            {
                DLLLoader.OpenLibrary();
                ExportFuncDef.Init();
                {
                    Texture2D tex = getTestTexture();
                    Color32[] cols = tex.GetPixels32();
                    V3F_C4B_T2F[] verts = null;
                    ushort[] indices = null;
                    unsafe
                    {
                        fixed (Color32* colData = cols)
                        {
                            ExportFuncDef.CppDele_AutoPolygonGenerate_DataCallback callback = (data, len, type) =>
                            {
                                EAutoPolygonDataType eAutoPolygonDataType = (EAutoPolygonDataType)type;
                                switch (eAutoPolygonDataType)
                                {
                                    case EAutoPolygonDataType.V3F_C4B_T2F:
                                        verts = new V3F_C4B_T2F[len];
                                        fixed (V3F_C4B_T2F* dataDst = verts)
                                        {
                                            DLLLoader.CopyMemory(new IntPtr(dataDst), data, (uint)len);
                                        }

                                        break;
                                    case EAutoPolygonDataType.INDEX:
                                        indices = new ushort[len];
                                        fixed (ushort* dataDst = indices)
                                        {
                                            DLLLoader.CopyMemory(new IntPtr(dataDst), data, (uint)len);
                                        }

                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                Debug.Log($"dataLen = {len} type = {type}");
                            };
                            IntPtr dataPtr = new IntPtr(colData);
                            int dataLen = cols.Length * 4;
                            int w = tex.width;
                            int h = tex.height;
                            var ret = ExportFuncDef._cppDele_AutoPolygonGenerate(
                                dataPtr,
                                dataLen,
                                w, h,
                                Marshal.StringToHGlobalAnsi(tex.name),
                                0f, 0f, 0f, 0f,
                                20.0f, 0f,
                                Marshal.GetFunctionPointerForDelegate(callback));
                            CreateGo(verts, indices);
                            Debug.LogError($"ret is {ret}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"!! error happend {e.Message}");
            }
            finally
            {
                ExportFuncDef.UnInit();
                DLLLoader.CloseLibrary();
            }
        }

        static void CreateGo(V3F_C4B_T2F[] verts, ushort[] indices)
        {
            var go = new GameObject();
            go.name = "testGenGo";
            var mf = go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            var mesh = new Mesh();
            List<Vector3> listVerts = new List<Vector3>();
            List<ushort> listIndex = new List<ushort>();
            List<Vector2> listUVs = new List<Vector2>();
            foreach (V3F_C4B_T2F v in verts)
            {
                listVerts.Add(new Vector3(v.vertices.x, v.vertices.y, v.vertices.z));
                listUVs.Add(new Vector2(v.texCoords.u, v.texCoords.v));
            }

            foreach (var i in indices)
            {
                listIndex.Add(i);
            }

            mesh.SetVertices(listVerts);
            mesh.SetUVs(0, listUVs);
            mesh.SetIndices(listIndex, MeshTopology.Triangles, 0);

            mf.mesh = mesh;
        }

        static Texture2D getTestTexture()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/TestImages/test1Image.png");
        }
    }
}