using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using App.Utils;
using App.Utils.InspectorEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoPolyMesh))]
public class YourMonoBehaviorEditor : Editor
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
        poly.epsilon = EditorGUILayout.Slider("epsilon", poly.epsilon, 1.0f, 100.0f);
        poly.alphaThreshold = EditorGUILayout.Slider("alphaThreshold", poly.alphaThreshold, 0f, 128f);
        if (EditorGUI.EndChangeCheck())
        {
            Debug.Log($"epsilon:{poly.epsilon}  alphaThreshold:{poly.alphaThreshold}");
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
        if (tex)
        {
            if (GetMeshData(tex, poly.epsilon, poly.alphaThreshold, _listVerts, _listUVs, _listIndex))
            {
                poly.UpdateMeshData(_listVerts, _listUVs, _listIndex);
                poly.UpdateMainTexture();
            }
        }
        else
        {
            Debug.LogError("没有得到texture2D 错误");
        }
    }

    private bool GetMeshData(Texture2D tex, float epsilon, float alpha_thresold, List<Vector3> listVerts, List<Vector2> listUVs, List<ushort> listIndex)
    {
        Color32[] cols = tex.GetPixels32();

        var ret = ExportFuncDef.AutoPolygonGenerat(cols, tex.width, tex.height, ConvertAssetPath2FullPath(tex), epsilon, alpha_thresold, listVerts, listUVs,
            listIndex);
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