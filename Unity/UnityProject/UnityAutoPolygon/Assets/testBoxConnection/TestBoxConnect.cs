using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UILineRenderer))]
[RequireComponent(typeof(Canvas))]
public class TestBoxConnect : MonoBehaviour
{
    private Canvas canvas;
    private UILineRenderer lineRenderer;
    private RectTransform RT;
    private Vector2 rectPos;
    private List<Vector2> points = new List<Vector2>();
    private int CurrentLine = 0;


    public Button btnClear;
    public List<Button> listButton;

    public Button btn1;
    public Button btn2;

    public int clickCount = 0;

    void Start()
    {
        canvas = GetComponent<Canvas>();
        lineRenderer = GetComponent<UILineRenderer>();
        RT = GetComponent<RectTransform>();
        rectPos = RT.position;
        for (int i = 0; i < listButton.Count; i++)
        {
            var btn = listButton[i];
            btn.onClick.AddListener(() => { OnBtn(btn); });
        }

        foreach (var btn in listButton)
        {
            btn.onClick.AddListener(() => { });
        }

        btnClear.onClick.AddListener(ClearLines);
    }

    private void ClearLines()
    {
        clickCount = 0;
        btn1 = null;
        btn2 = null;
        points.Clear();
        RefreshLine();
        Debug.Log("clear");
    }

    private void OnBtn(Button clickBtn)
    {
        if (clickCount == 0)
        {
            btn1 = clickBtn;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, clickBtn.transform.position);
            points.Add(screenPosition - rectPos);
            clickCount++;
        }
        else if (clickCount == 1)
        {
            btn2 = clickBtn;
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, clickBtn.transform.position);
            points.Add(screenPosition - rectPos);
            RefreshLine();
            clickCount++;
        

        }
    }


    private void RefreshLine()
    {
        lineRenderer.Points = points.ToArray();
        lineRenderer.SetAllDirty();
    }
}


public class SquareBox
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
}

public class Port
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class LineSegment
{
    public float StartX { get; set; }
    public float StartY { get; set; }
    public float EndX { get; set; }
    public float EndY { get; set; }

    public LineSegment(float startX, float startY, float endX, float endY)
    {
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
    }
}

public static class LineAlgorithm
{
    public static List<LineSegment> ConnectPorts(Port port1, Port port2, SquareBox box1, SquareBox box2)
    {
        List<LineSegment> lineSegments = new List<LineSegment>();

        // 连接两个端口
        lineSegments.AddRange(ConnectPortToPort(port1, port2, box1, box2));

        return lineSegments;
    }

    private static List<LineSegment> ConnectPortToPort(Port port1, Port port2, SquareBox box1, SquareBox box2)
    {
        List<LineSegment> lineSegments = new List<LineSegment>();

        // 垂直连接
        lineSegments.Add(new LineSegment(port1.X, port1.Y, port1.X, port2.Y));
        lineSegments.Add(new LineSegment(port1.X, port2.Y, port2.X, port2.Y));

        // 避免与盒子重叠
        if (box1 != null && !IsLineOverlappingBox(lineSegments, box1))
        {
            lineSegments.Insert(0, new LineSegment(port1.X, port1.Y, port1.X, box1.Y));
            lineSegments.Insert(1, new LineSegment(port1.X, box1.Y, port2.X, box1.Y));
            lineSegments.Insert(2, new LineSegment(port2.X, box1.Y, port2.X, port2.Y));
        }

        if (box2 != null && !IsLineOverlappingBox(lineSegments, box2))
        {
            lineSegments[1] = new LineSegment(port1.X, port2.Y, port2.X, port2.Y);
            lineSegments.Add(new LineSegment(port2.X, port2.Y, port2.X, box2.Y));
        }

        return lineSegments;
    }

    private static bool IsLineOverlappingBox(List<LineSegment> lineSegments, SquareBox box)
    {
        foreach (var segment in lineSegments)
        {
            if (IsSegmentOverlappingBox(segment, box))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSegmentOverlappingBox(LineSegment segment, SquareBox box)
    {
        return !(segment.EndX < box.X || segment.StartX > box.X + box.Width ||
                 segment.EndY < box.Y || segment.StartY > box.Y + box.Height);
    }
}