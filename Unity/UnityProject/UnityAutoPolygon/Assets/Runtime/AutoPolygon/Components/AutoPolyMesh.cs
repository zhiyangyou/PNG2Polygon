using System;
using System.Collections.Generic;
using App.Utils.InspectorEditor;
using UnityEngine;


[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class AutoPolyMesh : MonoBehaviour
{
    MeshFilter _meshFilter = null;
    MeshRenderer _meshRenderer = null;
    Mesh _mesh = null;

    #region serializeData

    [HideInInspector] [SerializeField] List<Vector3> _verts = new List<Vector3>();
    [HideInInspector] [SerializeField] List<Vector2> _uvs = new List<Vector2>();
    [HideInInspector] [SerializeField] List<ushort> _indices = new List<ushort>();

    [HideInInspector] [SerializeField] public float epsilon = 1.0f;
    [HideInInspector] [SerializeField] public float alphaThreshold = 0.0f;
    [SerializeField] public Texture2D texMaintexture = null;

    #endregion

    private const string meshName = "gen_by_auto_poly";

    Mesh mesh
    {
        get
        {
            if (!_mesh)
            {
                _mesh = new Mesh();
            }

            return _mesh;
        }
    }

    MeshFilter meshFilter
    {
        get
        {
            if (!_meshFilter)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            return _meshFilter;
        }
    }

    MeshRenderer meshRenderer
    {
        get
        {
            if (!_meshRenderer)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }

            return _meshRenderer;
        }
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            RebuildMeshData();
        }
    }

    void RebuildMeshData()
    {
        mesh.Clear();
        mesh.SetVertices(_verts);
        mesh.SetUVs(0, _uvs);
        mesh.SetIndices(_indices, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.name = $"{meshName}-{_indices.Count / 3}";
    }

    #region public

    public void UpdateMainTexture()
    {
        if (meshRenderer.sharedMaterial)
        {
            meshRenderer.sharedMaterial.mainTexture = this.texMaintexture;
        }
    }

    public void UpdateMeshData(List<Vector3> vs, List<Vector2> uvs, List<ushort> indices)
    {
        if (vs.Count != uvs.Count || indices.Count % 3 != 0)
        {
            Debug.LogError(
                $"UpdateMeshData 错误1，非法的数据 vs.Count != uvs.Count || indices.Count % 3 != 0  vs.Count: {vs.Count} uvs.Count: {uvs.Count} indices.Count: {indices.Count}");
            return;
        }

        if (vs.Count > 0 && uvs.Count > 0 && indices.Count > 0)
        {
            this._verts.Clear();
            this._verts.AddRange(vs);

            this._uvs.Clear();
            this._uvs.AddRange(uvs);

            this._indices.Clear();
            this._indices.AddRange(indices);

            RebuildMeshData();
            meshFilter.mesh = mesh;
        }
    }

    #endregion

#if UNITY_EDITOR


    [NonSerialized] public bool isDrawGizmos = false;
    [NonSerialized] public float reduceOverDraw = 12.345f;

    private void OnDrawGizmos()
    {
        if (isDrawGizmos)
        {
            GizmosUtils.DrawMeshTriLines(_verts, _indices, transform);
            GizmosUtils.DrawMeshTriArea(_verts, _indices, transform);
            GizmosUtils.DrawInfos(_indices.Count / 3, reduceOverDraw, transform);
        }
    }

#endif
}