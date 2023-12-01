using System.Collections.Generic;
using Runtime.AutoPolygon;
using UnityEngine;

namespace App.Utils.Utils
{
    public static class GeometryUtils
    {
        private static List<ushort> _listIndexInverseTempBuffer = new List<ushort>();

        public static void FillData(V3F_C4B_T2F[] verts, ushort[] indices, List<Vector3> listVerts, List<Vector2> listUVs, List<ushort> listIndex)
        {
            listVerts.Clear();
            listUVs.Clear();
            listIndex.Clear();
            foreach (V3F_C4B_T2F v in verts)
            {
                listVerts.Add(new Vector3(v.vertices.x, v.vertices.y, v.vertices.z));
                listUVs.Add(new Vector2(v.texCoords.u, v.texCoords.v));
            }

            foreach (var i in indices)
            {
                listIndex.Add(i);
            }
        }


        public static void InverseUV_V(List<Vector2> uvs)
        {
            List<Vector2> uvTemp = new List<Vector2>(uvs.Count);
            foreach (var uv in uvs)
            {
                uvTemp.Add(new Vector2(uv.x, 1.0f - uv.y));
            }

            uvs.Clear();
            uvs.AddRange(uvTemp);
        }

        public static void InverseXY_Y(List<Vector3> verts, float H)
        {
            List<Vector3> vTemp = new List<Vector3>(verts.Count);
            foreach (var uv in verts)
            {
                vTemp.Add(new Vector3(uv.x, H - uv.y, 0f));
            }

            verts.Clear();
            verts.AddRange(vTemp);
        }

        public static void InverseTriangle(List<ushort> listIndex)
        {
            if (listIndex.Count % 3 != 0)
            {
                Debug.LogError("错误的三角形数据 index数量不是3的倍数");
                return;
            }

            _listIndexInverseTempBuffer.Clear();
            _listIndexInverseTempBuffer.AddRange(listIndex);
            listIndex.Clear();
            for (int i = 0; i < _listIndexInverseTempBuffer.Count; i += 3)
            {
                listIndex.Add(_listIndexInverseTempBuffer[i + 2]);
                listIndex.Add(_listIndexInverseTempBuffer[i + 1]);
                listIndex.Add(_listIndexInverseTempBuffer[i]);
            }
        }
    }
}