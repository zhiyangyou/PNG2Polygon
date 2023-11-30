#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace App.Utils.InspectorEditor
{
    public static class GizmosUtils
    {
        public static void DrawMeshTriLines(List<Vector3> vertices, List<ushort> triangles, Transform tr)
        {
            Color oldGizmosColor = Gizmos.color;
            Matrix4x4 oldMat44 = Gizmos.matrix;
            Gizmos.color = Color.blue;
            Gizmos.matrix = tr.localToWorldMatrix;
            for (int i = 0; i < triangles.Count; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                Gizmos.DrawLine(v0, v1);
                Gizmos.DrawLine(v1, v2);
                Gizmos.DrawLine(v2, v0);
            }

            Gizmos.color = oldGizmosColor;
            Gizmos.matrix = oldMat44;
        }

        public static void DrawInfos(int triNum, float overDrawNum, Transform tr)
        {
            Color oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = new Color(1f, 1f, 1f, 1f);

            UnityEditor.Handles.Label(tr.TransformPoint(Vector3.down * 20f), $"{overDrawNum.ToString("F1")}%过渡绘制，{triNum}个三角形");

            UnityEditor.Handles.color = oldColor;
        }

        public static void DrawMeshTriArea(List<Vector3> vertices, List<ushort> triangles, Transform transform)
        {
            Color oldColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = new Color(0f, 0f, 1f, 0.3f);

            for (int i = 0; i < triangles.Count; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];
                UnityEditor.Handles.DrawAAConvexPolygon(
                    transform.TransformPoint(v0),
                    transform.TransformPoint(v1),
                    transform.TransformPoint(v2)
                );
            }

            UnityEditor.Handles.color = oldColor;
        }
    }
}
#endif