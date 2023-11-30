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