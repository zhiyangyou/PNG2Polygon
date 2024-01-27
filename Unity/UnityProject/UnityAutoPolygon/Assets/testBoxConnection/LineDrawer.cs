using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class LineDrawer : Graphic
{
    public Vector2 startPoint;
    public Vector2 endPoint;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // 构造线段的顶点数据
        UIVertex vertex1 = new UIVertex();
        vertex1.position = startPoint;
        vertex1.color = color;
        vh.AddVert(vertex1);

        UIVertex vertex2 = new UIVertex();
        vertex2.position = endPoint;
        vertex2.color = color;
        vh.AddVert(vertex2);

        // 构造线段的三角形数据
        vh.AddTriangle(0, 1, 1);
    }
}