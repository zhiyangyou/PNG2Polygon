#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AutoPolygon
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

        public static void DrawInfos(int triNum, float curOverDraw, float minOverDraw, Transform tr)
        {
            var drawPos = tr.TransformPoint(Vector3.zero);
            Color oldColor = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 1f);
            Handles.Label(drawPos + Vector3.down * 20f, $"{(curOverDraw * 100f).ToString("F1")}%过渡绘制，{triNum}个三角形");
            Handles.color = oldColor;

            float offsetY = 0f;
            var totalLen = 200f;
            var height = 10f;
            var curLen = curOverDraw * totalLen;
            var minLen = minOverDraw * totalLen;
            DrawMeshRect(tr, Vector2.zero + Vector2.down * 50f, totalLen, height, new Color(56/255f, 56/255f, 56/255f, 1f));
            DrawMeshRect(tr, Vector2.zero + Vector2.down * 50f, curLen, height, new Color(0, 1, 0, 0.8f));
            DrawMeshRect(tr, Vector2.zero + Vector2.down * 50f, minLen, height, new Color(0, 0, 1f, 0.8f));
        }

        public static void DrawMeshTriArea(List<Vector3> vertices, List<ushort> triangles, Transform transform)
        {
            Color oldColor = Handles.color;
            Handles.color = new Color(0f, 0f, 255f, 0.07f);

            for (int i = 0; i < triangles.Count; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];
                Handles.DrawAAConvexPolygon(
                    transform.TransformPoint(v0),
                    transform.TransformPoint(v1),
                    transform.TransformPoint(v2)
                );
            }

            Handles.color = oldColor;
        }

        public static void DrawMeshRect(Transform transform, Vector2 offset, float width, float height, Color col)
        {
            Color oldColor = Handles.color;
            Handles.color = col;

            Vector3 LeftMiddle = transform.TransformPoint(0 + offset.x, height / 2f + offset.y, 0);
            Vector3 LB = LeftMiddle + new Vector3(0, -height / 2f);
            Vector3 LT = LeftMiddle + new Vector3(0, height / 2f);
            Vector3 RB = LeftMiddle + new Vector3(width, -height / 2f);
            Vector3 RT = LeftMiddle + new Vector3(width, height / 2f);
            Handles.DrawAAConvexPolygon(LB, LT, RT);
            Handles.DrawAAConvexPolygon(LB, RT, RB);
            Handles.color = oldColor;
        }
    }
}
#endif