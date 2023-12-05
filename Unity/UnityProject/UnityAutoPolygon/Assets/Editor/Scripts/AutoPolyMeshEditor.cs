using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AutoPolygon
{
    [CustomEditor(typeof(AutoPolyMesh))]
    public class AutoPolyMeshEditor : Editor
    {
        List<Vector3> _listVerts = new List<Vector3>();
        List<Vector2> _listUVs = new List<Vector2>();
        List<ushort> _listIndex = new List<ushort>();

        private void OnEnable()
        {
            // DLLLoader.OpenLibrary();
            // ExportFuncDef.Init();
            AutoPolyMesh poly = (AutoPolyMesh)target;
            poly.isDrawGizmos = true;
        }

        private void OnDisable()
        {
            AutoPolyMesh poly = (AutoPolyMesh)target;
            poly.isDrawGizmos = false;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            AutoPolyMesh poly = (AutoPolyMesh)target;
            EditorGUI.BeginChangeCheck();

            poly.isDrawGizmos = EditorGUILayout.Toggle("gizmos", poly.isDrawGizmos);
            poly.epsilon = EditorGUILayout.Slider("epsilon", poly.epsilon, 3.0f, 100.0f);
            poly.alphaThreshold = EditorGUILayout.Slider("alphaThreshold", poly.alphaThreshold, 0f, 128f);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshMesh();
                EditorUtility.SetDirty(poly);
            }

            if (GUI.Button(EditorGUILayout.GetControlRect(), "RefreshMesh"))
            {
                RefreshMesh();
            }
        }

        private void RefreshMesh()
        {
            AutoPolyMesh poly = (AutoPolyMesh)target;
            Texture2D tex = poly.texMaintexture as Texture2D;
            Overdrawinfo info = new Overdrawinfo();
            if (tex)
            {
                if (GetMeshData(tex, poly.epsilon, poly.alphaThreshold, _listVerts, _listUVs, _listIndex, ref info))
                {
                    poly.UpdateMeshData(_listVerts, _listUVs, _listIndex);
                    poly.UpdateMainTexture();
                }

                poly.CurOverDraw = info.TriArea / (float)info.totalArea;
                poly.MinOverDraw = info.pixelArea / (float)info.totalArea;
            }
            else
            {
                Debug.LogError("没有得到texture2D 错误");
            }
        }

        private bool GetMeshData(Texture2D tex, float epsilon, float alpha_thresold, List<Vector3> listVerts, List<Vector2> listUVs, List<ushort> listIndex,
            ref Overdrawinfo info)
        {
            var ret = ExportFuncDef.AutoPolygonGenerateByPNGFullPath(ConvertAssetPath2FullPath(tex), epsilon, alpha_thresold, listVerts, listUVs, listIndex,
                ref info);
            return ret == 0;
        }

        private string ConvertAssetPath2FullPath(UnityEngine.Object obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            return ConvertAssetPath2FullPath(assetPath);
        }

        private string ConvertAssetPath2FullPath(string assetPath)
        {
            return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        }
    }
}