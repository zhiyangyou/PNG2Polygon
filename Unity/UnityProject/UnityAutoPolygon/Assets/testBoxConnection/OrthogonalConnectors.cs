using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace testBoxConnection
{
    public enum BasicCardinalPoint : int
    {
        n,
        e,
        s,
        w
    }

    public enum Direction
    {
        v,
        h
    }

    public enum Side
    {
        top,
        right,
        bottom,
        left
    }

    public enum BendDirection
    {
        n,
        e,
        s,
        w,
        unknown,
        none
    }

    public struct Point
    {
        public float x;
        public float y;

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"({x},{y})";
        }
    }

    public struct Size
    {
        public float width;
        public float height;

        public Size(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
    }

    public struct Line
    {
        public Point a;
        public Point b;

        public Line(Point a, Point b)
        {
            this.a = a;
            this.b = b;
        }
    }

    public struct Rect
    {
        public float left;
        public float top;
        public float width;
        public float height;

        public Rect(float left, float top, float width, float height)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
        }
    }

    public class ConnectorPoint
    {
        public Rect shape;
        public Side side;
        public float distance;
    }

    public class OrthogonalConnectorByproduct
    {
        public List<float> hRulers = null;
        public List<float> vRulers = null;
        public List<Point> spots = null;
        public List<Rectangle> grid = null;
        public List<Line> connections = null;
    }

    public class OrthogonalConnectorOpts
    {
        public ConnectorPoint pointA;
        public ConnectorPoint pointB;
        public float shapeMargin;
        public float globalBoundsMargin;
        public Rect globalBounds;
    }

    public class Rectangle
    {
        public float left { get; private set; }
        public float top { get; private set; }
        public float width { get; private set; }
        public float height { get; private set; }

        public static Rectangle get_empty()
        {
            return new Rectangle(0, 0, 0, 0);
        }

        public static Rectangle fromRect(Rect r)
        {
            return new Rectangle(r.left, r.top, r.width, r.height);
        }

        public static Rectangle fromLTRB(float left, float top, float right, float bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public Rectangle(float left, float top, float width, float height)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
        }

        public bool contains(Point p)
        {
            return p.x >= this.left && p.x <= this.right && p.y >= this.top && p.y <= this.bottom;
        }

        //膨胀
        public Rectangle inflate(float horizontal, float vertical)
        {
            return fromLTRB(this.left - horizontal, this.top - vertical, this.right + horizontal, this.bottom + vertical);
        }

        //相交测试
        public bool intersects(Rectangle rectangle)
        {
            var thisX = this.left;
            var thisY = this.top;
            var thisW = this.width;
            var thisH = this.height;
            var rectX = rectangle.left;
            var rectY = rectangle.top;
            var rectW = rectangle.width;
            var rectH = rectangle.height;
            return (rectX < thisX + thisW)
                   && (thisX < (rectX + rectW))
                   && (rectY < thisY + thisH)
                   && (thisY < rectY + rectH);
        }

        //求合并
        public Rectangle union(Rectangle r)
        {
            var minX = UtilFuncs.GetMin(this.left, this.right, r.left, r.right);
            var maxX = UtilFuncs.GetMax(this.left, this.right, r.left, r.right);
            var minY = UtilFuncs.GetMin(this.top, this.bottom, r.top, r.bottom);
            var maxY = UtilFuncs.GetMin(this.top, this.bottom, r.top, r.bottom);
            return Rectangle.fromLTRB(minX, minY, maxX, maxY);
        }


        public Point center =>
            new(
                x:
                this.left + this.width / 2,
                y:
                this.top + this.height / 2
            );


        public float right => this.left + this.width;

        public float bottom => this.top + this.height;


        public Point location => UtilFuncs.makePt(this.left, this.top);
        public Point northEast => new(this.right, this.top);
        public Point southEast => new(this.right, this.bottom);
        public Point southWest => new(this.left, this.bottom);
        public Point northWest => new(this.left, this.top);
        public Point east => UtilFuncs.makePt(this.right, this.center.y);
        public Point north => UtilFuncs.makePt(this.center.x, this.top);
        public Point south => UtilFuncs.makePt(this.center.x, this.bottom);
        public Point west => UtilFuncs.makePt(this.left, this.center.y);

        public Size size
        {
            get { return new Size(this.width, this.height); }
        }
    }

    public class PointNode
    {
        public float distance = float.MaxValue;
        public PointNode[] shortestPath;
        public Point data;

        /// <summary>
        /// 邻近节点
        /// </summary>
        /// <returns></returns>
        public Dictionary<PointNode, float> adjacentNodes = new();


        public PointNode(Point data)
        {
            this.data = data;
        }

        public override string ToString()
        {
            return $"PonitNode:{data.ToString()}";
        }
    }

    public class PointGraph
    {
        public class _Index
        {
            private Dictionary<string, Dictionary<string, PointNode>> _dicIndex = new();


            public bool TryGetPointNode(string xs, string ys, out PointNode node)
            {
                node = null;
                if (_dicIndex.TryGetValue(xs, out var dic))
                {
                    return dic.TryGetValue(ys, out node);
                }

                return false;
            }

            public bool ContainKeys(float x, float y)
            {
                return ContainKeys(x.ToString(), y.ToString());
            }

            public bool ContainKeys(string xs, string ys)
            {
                if (_dicIndex.TryGetValue(xs, out var dic))
                {
                    return dic.ContainsKey(ys);
                }

                return false;
            }

            public void AddToIndex(string x, string y, PointNode pointNode)
            {
                Dictionary<string, PointNode> valueDic = null;
                if (!_dicIndex.ContainsKey(x))
                {
                    valueDic = new Dictionary<string, PointNode>();
                    _dicIndex.Add(x, valueDic);
                }
                else
                {
                    valueDic = _dicIndex[x];
                }

                if (!valueDic.ContainsKey(y))
                {
                    valueDic.Add(y, pointNode);
                }
                else
                {
                    throw new Exception($"has already added to dic {x} {y} {pointNode}");
                }
            }
        }

        private _Index index = new _Index();
        // private index: {[x: string]: {[y: string]: PointNode}} = {};

        public void add(Point p)
        {
            // const {x, y} = p;
            float x = p.x;
            float y = p.y;
            string xs = x.ToString();
            string ys = y.ToString();
            index.AddToIndex(xs, ys, new PointNode(p));
        }

        [CanBeNull]
        private PointNode getLowestDistanceNode(HashSet<PointNode> unsettledNodes)
        {
            PointNode? lowestDistanceNode = null;
            float lowestDistance = float.MaxValue;
            foreach (PointNode node in unsettledNodes)
            {
                var nodeDistance = node.distance;
                if (nodeDistance < lowestDistance)
                {
                    lowestDistance = nodeDistance;
                    lowestDistanceNode = node;
                }
            }

            return lowestDistanceNode!;
        }

        private Direction? inferPathDirection(PointNode node)
        {
            if (node.shortestPath.Length == 0)
            {
                return null;
            }

            return this.directionOfNodes(node.shortestPath[node.shortestPath.Length - 1], node);
        }

        public PointGraph calculateShortestPathFromSource(PointGraph graph, PointNode source)
        {
            source.distance = 0;
            HashSet<PointNode> settledNodes = new();
            HashSet<PointNode> unsettledNodes = new();
            unsettledNodes.Add(source);

            while (unsettledNodes.Count != 0)
            {
                PointNode currentNode = this.getLowestDistanceNode(unsettledNodes);
                unsettledNodes.Remove(currentNode);

                foreach (var kv in currentNode.adjacentNodes)
                {
                    var adjacentNode = kv.Key;
                    var edgeWeight = kv.Value;
                    if (!settledNodes.Contains(adjacentNode))
                    {
                        this.calculateMinimumDistance(adjacentNode, edgeWeight, currentNode);
                        unsettledNodes.Add(adjacentNode);
                    }
                }

                settledNodes.Add(currentNode);
            }

            return graph;
        }

        private void calculateMinimumDistance(PointNode evaluationNode, float edgeWeigh, PointNode sourceNode)
        {
            float sourceDistance = sourceNode.distance;
            Direction? comingDirection = this.inferPathDirection(sourceNode);
            Direction? goingDirection = this.directionOfNodes(sourceNode, evaluationNode);
            bool changingDirection = comingDirection != null
                                     && goingDirection != null
                                     && comingDirection != goingDirection;

            var extraWeigh = changingDirection != null ? Mathf.Pow(edgeWeigh + 1f, 2) : 0f;

            if (sourceDistance + edgeWeigh + extraWeigh < evaluationNode.distance)
            {
                evaluationNode.distance = sourceDistance + edgeWeigh + extraWeigh;
                // const shortestPath: PointNode[] = [...sourceNode.shortestPath]; js语义，数组浅拷贝
                List<PointNode> shortestPath = new();
                shortestPath.AddRange(sourceNode.shortestPath);
                shortestPath.Add(sourceNode);
                evaluationNode.shortestPath = shortestPath.ToArray();
            }
        }

        private Direction? directionOf(Point a, Point b)
        {
            if (Math.Abs(a.x - b.x) < 0.001f)
            {
                return Direction.h;
            }
            else if (Math.Abs(a.y - b.y) < 0.001f)
            {
                return Direction.v;
            }
            else
            {
                return null;
            }
        }

        private Direction? directionOfNodes(PointNode a, PointNode b)
        {
            return this.directionOf(a.data, b.data);
        }

        public void connect(Point a, Point b)
        {
            PointNode nodeA = this.get(a);
            PointNode nodeB = this.get(b);

            if (nodeA == null || nodeB == null)
            {
                throw new Exception($"{nodeA}  or {nodeB}point was not found)");
            }

            nodeA.adjacentNodes.set(nodeB, UtilFuncs.distance(a, b));
        }

        public bool has(Point p)
        {
            return index.ContainKeys(p.x, p.y);
        }

        [CanBeNull]
        public PointNode get(Point p)
        {
            var x = p.x;
            var y = p.y;
            string xs = x.ToString();
            string ys = y.ToString();

            if (index.TryGetPointNode(xs, ys, out var node))
            {
                return node;
            }

            return null;
        }
    }

    /**
 * Helps create the grid portion of the algorithm
 */
    public class Grid
    {
        public class GridData
        {
            public Dictionary<int, Dictionary<int, Rectangle>> DicDicDatas = new();

            public Dictionary<int, Rectangle> ensureGet(int row)
            {
                if (DicDicDatas.TryGetValue(row, out var existDic))
                {
                    return existDic;
                }
                else
                {
                    var newDic = new Dictionary<int, Rectangle>();
                    DicDicDatas.Add(row, newDic);
                    return newDic;
                }
            }

            [CanBeNull]
            public Dictionary<int, Rectangle> get(int row)
            {
                if (DicDicDatas.TryGetValue(row, out var ret))
                {
                    return ret;
                }

                return null;
            }

            [CanBeNull]
            public Rectangle get(int row, int column)
            {
                var rowMap = get(row);

                if (rowMap != null)
                {
                    return rowMap.get(column);
                }

                return null;
            }
        }

        private int _rows = 0;
        private int _cols = 0;

        public GridData data { get; private set; } = new GridData();

        public Grid()
        {
        }

        public void set(int row, int column, Rectangle rectangle)
        {
            this._rows = Mathf.Max(this.rows, row + 1);
            this._cols = Mathf.Max(this.columns, column + 1);

            var rowMap = data.ensureGet(row);

            rowMap.set2(column, rectangle);
        }


        public List<Rectangle> rectangles()
        {
            // const r  : Rectangle[] =  [] ;
            var r = new List<Rectangle>();

            foreach (var kv in data.DicDicDatas)
            {
                var row = kv.Key;
                var rowMap = kv.Value;
                foreach (var kvRow in rowMap)
                {
                    var col = kvRow.Key;
                    var rectTangle = kvRow.Value;
                    r.Add(rectTangle);
                }
            }

            return r;
        }

        public int columns => _cols;

        public int rows => _rows;
    }


    public static class UtilFuncs
    {
        /// <summary>
        /// 相当于js中的set，其实就是AddOrUpdate
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public static void set(this Dictionary<PointNode, float> dic, PointNode node, float value)
        {
            if (dic.ContainsKey(node))
            {
                dic[node] = value;
            }
            else
            {
                dic.Add(node, value);
            }
        }

        [CanBeNull]
        public static Rectangle get(this Dictionary<int, Rectangle> dic, int index)
        {
            if (dic.TryGetValue(index, out var ret))
            {
                return ret;
            }

            return null;
        }

        public static void set2(this Dictionary<int, Rectangle> dic, int index, Rectangle rectangle)
        {
            if (dic.ContainsKey(index))
            {
                dic[index] = rectangle;
            }
            else
            {
                dic.Add(index, rectangle);
            }
        }

        public static float GetMin(params float[] xxxx)
        {
            float min = float.MaxValue;
            foreach (var x in xxxx)
            {
                if (x < min)
                {
                    min = x;
                }
            }

            return min;
        }

        public static float GetMax(params float[] xxxx)
        {
            float max = float.MinValue;
            foreach (var x in xxxx)
            {
                if (x > max)
                {
                    max = x;
                }
            }

            return max;
        }

        public static Point makePt(float x, float y)
        {
            return new Point(x, y);
        }

        public static float distance(Point a, Point b)
        {
            return Mathf.Sqrt(Mathf.Pow(b.x - a.x, 2) + Mathf.Pow(b.y - a.y, 2));
        }


        public static Point computePt(ConnectorPoint p)
        {
            var b = Rectangle.fromRect(p.shape);
            switch (p.side)
            {
                case Side.top:
                    return makePt(b.left + b.width * p.distance, b.top);
                case Side.right:
                    return makePt(b.right, b.top + b.height * p.distance);
                case Side.bottom:
                    return makePt(b.left + b.width * p.distance, b.bottom);
                case Side.left:
                    return makePt(b.left, b.top + b.height * p.distance);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static Point extrudeCp(ConnectorPoint cp, float margin)
        {
            var pt = computePt(cp);
            var x = pt.x;
            var y = pt.y;
            switch (cp.side)
            {
                case Side.top:
                    return makePt(x, y - margin);
                case Side.right:
                    return makePt(x + margin, y);
                case Side.bottom:
                    return makePt(x, y + margin);
                case Side.left:
                    return makePt(x - margin, y);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static bool isVerticalSide(Side side)
        {
            // return side == "top" || side == "bottom";
            return side == Side.top || side == Side.bottom;
        }

        public static Grid rulersToGrid(List<float> verticals, List<float> horizontals, Rectangle bounds)
        {
            Grid result = new Grid();

            //TODO js 的 C#的顺序一致？
            verticals.Sort((a, b) => (a - b) > 0f ? 1 : -1);
            horizontals.Sort((a, b) => (a - b) > 0f ? 1 : -1);

            var lastX = bounds.left;
            var lastY = bounds.top;
            var column = 0;
            var row = 0;

            // for ( const y of horizontals)
            foreach (int y in horizontals)
            {
                // for ( const x of verticals)
                foreach (int x in verticals)
                {
                    result.set(row, column++, Rectangle.fromLTRB(lastX, lastY, x, y));
                    lastX = x;
                }

                // Last cell of the row
                result.set(row, column, Rectangle.fromLTRB(lastX, lastY, bounds.right, y));
                lastX = bounds.left;
                lastY = y;
                column = 0;
                row++;
            }

            lastX = bounds.left;

            // Last fow of cells
            // for ( const x of verticals) 
            foreach (int x in verticals)
            {
                result.set(row, column++, Rectangle.fromLTRB(lastX, lastY, x, bounds.bottom));
                lastX = x;
            }

            // Last cell of last row
            result.set(row, column, Rectangle.fromLTRB(lastX, lastY, bounds.right, bounds.bottom));

            return result;
        }


        public static Point[] reducePoints(Point[] points)
        {
            var result = new List<Point>();
            var map = new Dictionary<float, List<float>>();

            foreach (var p in points)
            {
                var x = p.x;
                var y = p.y;


                List<float> arr = null;
                if (map.TryGetValue(y, out var arrInDic))
                {
                    arr = arrInDic;
                }
                else
                {
                    arr = new List<float>();
                    map.Add(y, arr);
                }

                if (arr.IndexOf(x) < 0)
                {
                    arr.Add(x);
                }
            }

            foreach (var kv in map)
            {
                var y = kv.Key;
                var xs = kv.Value;

                foreach (var x in xs)
                {
                    result.Add(makePt(x, y));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="obstacles">障碍物</param>
        /// <returns></returns>
        public static Point[] gridToSpots(Grid grid, Rectangle[] obstacles)
        {
            //障碍物检测函数
            // const obstacleCollision = (p: Point) => obstacles.filter(o => o.contains(p)).length > 0;
            Func<Point, bool> obstacleCollision = delegate(Point p) { return obstacles.Any(o => o.contains(p)); };

            List<Point> gridPoints = new();

            foreach (var kv in grid.data.DicDicDatas)
            {
                var row = kv.Key;
                var data = kv.Value;
                bool firstRow = row == 0;
                bool lastRow = row == grid.rows - 1;

                foreach (var kvInData in data)
                {
                    // const [col, r]
                    var col = kvInData.Key;
                    var r = kvInData.Value;
                    bool firstCol = col == 0;
                    bool lastCol = col == grid.columns - 1;
                    var nw = firstCol && firstRow;
                    var ne = firstRow && lastCol;
                    var se = lastRow && lastCol;
                    var sw = lastRow && firstCol;

                    if (nw || ne || se || sw)
                    {
                        gridPoints.Add(r.northWest);
                        gridPoints.Add(r.northEast);
                        gridPoints.Add(r.southWest);
                        gridPoints.Add(r.southEast);
                    }
                    else if (firstRow)
                    {
                        gridPoints.Add(r.northWest);
                        gridPoints.Add(r.north);
                        gridPoints.Add(r.northEast);
                    }
                    else if (lastRow)
                    {
                        gridPoints.Add(r.southEast);
                        gridPoints.Add(r.south);
                        gridPoints.Add(r.southWest);
                    }
                    else if (firstCol)
                    {
                        gridPoints.Add(r.northWest);
                        gridPoints.Add(r.west);
                        gridPoints.Add(r.southWest);
                    }
                    else if (lastCol)
                    {
                        gridPoints.Add(r.northEast);
                        gridPoints.Add(r.east);
                        gridPoints.Add(r.southEast);
                    }
                    else
                    {
                        gridPoints.Add(r.northWest);
                        gridPoints.Add(r.north);
                        gridPoints.Add(r.northEast);
                        gridPoints.Add(r.east);
                        gridPoints.Add(r.southEast);
                        gridPoints.Add(r.south);
                        gridPoints.Add(r.southWest);
                        gridPoints.Add(r.west);
                        gridPoints.Add(r.center);
                    }
                }
            }


            // return reducePoints(gridPoints).filter(p => !obstacleCollision(p));
            return reducePoints(gridPoints.ToArray()).Where(p => !obstacleCollision(p)).ToArray();
        }


        public static (PointGraph, List<Line>) createGraph(List<Point> spots)
        {
            List<float> hotXs = new();
            List<float> hotYs = new();
            var graph = new PointGraph();
            List<Line> connections = new();

            foreach (var p in spots)
            {
                var x = p.x;
                var y = p.y;
                if (hotXs.IndexOf(x) < 0) hotXs.Add(x);
                if (hotYs.IndexOf(y) < 0) hotYs.Add(y);
                graph.add(p);
            }

            //TODO 确认sort结果和js一致
            hotXs.Sort((a, b) => (a - b) > 0f ? 1 : -1);
            hotYs.Sort((a, b) => (a - b) > 0f ? 1 : -1);

            Func<Point, bool> inHotIndex = (Point p) => graph.has(p);

            for (var i = 0; i < hotYs.Count; i++)
            {
                for (var j = 0; j < hotXs.Count; j++)
                {
                    var b = makePt(hotXs[j], hotYs[i]);

                    if (!inHotIndex(b)) continue;

                    if (j > 0)
                    {
                        var a = makePt(hotXs[j - 1], hotYs[i]);

                        if (inHotIndex(a))
                        {
                            graph.connect(a, b);
                            graph.connect(b, a);
                            connections.Add(new Line(a, b));
                        }
                    }

                    if (i > 0)
                    {
                        var a = makePt(hotXs[j], hotYs[i - 1]);

                        if (inHotIndex(a))
                        {
                            graph.connect(a, b);
                            graph.connect(b, a);
                            connections.Add(new Line(a, b));
                        }
                    }
                }
            }

            return (graph, connections);
        }


        public static Point[] shortestPath(PointGraph graph, Point origin, Point destination)
        {
            var originNode = graph.get(origin);
            var destinationNode = graph.get(destination);

            if (originNode == null)
            {
                throw new Exception($"Origin node {origin.x},{origin.y} not found");
            }

            if (destinationNode == null)
            {
                throw new Exception($"Origin node {origin.x},{origin.y}not found");
            }

            graph.calculateShortestPathFromSource(graph, originNode);

            return destinationNode.shortestPath.Select(n => n.data).ToArray();
        }

        public static BendDirection getBend(Point a, Point b, Point c)
        {
            var equalX = Math.Abs(a.x - b.x) < 0.0001f && Math.Abs(b.x - c.x) < 0.0001f;
            var equalY = Math.Abs(a.y - b.y) < 0.0001f && Math.Abs(b.y - c.y) < 0.0001f;
            var segment1Horizontal = Math.Abs(a.y - b.y) < 0.0001f;
            var segment1Vertical = Math.Abs(a.x - b.x) < 0.0001f;
            var segment2Horizontal = Math.Abs(b.y - c.y) < 0.0001f;
            var segment2Vertical = Math.Abs(b.x - c.x) < 0.0001f;

            if (equalX || equalY)
            {
                return BendDirection.none;
            }

            if (
                !(segment1Vertical || segment1Horizontal) ||
                !(segment2Vertical || segment2Horizontal)
            )
            {
                return BendDirection.unknown;
            }

            if (segment1Horizontal && segment2Vertical)
            {
                return c.y > b.y ? BendDirection.s : BendDirection.n;
            }
            else if (segment1Vertical && segment2Horizontal)
            {
                return c.x > b.x ? BendDirection.e : BendDirection.w;
            }

            throw new Exception("Nope 错误的BendDirection结果，这是bug");
        }


        public static List<Point> simplifyPath(List<Point> points)
        {
            if (points.Count <= 2)
            {
                return points.ToList();
            }

            List<Point> r = new();
            r.Add(points[0]);
            for (var i = 1; i < points.Count; i++)
            {
                var cur = points[i];

                if (i == (points.Count - 1))
                {
                    r.Add(cur);
                    break;
                }

                var prev = points[i - 1];
                var next = points[i + 1];
                var bend = getBend(prev, cur, next);

                if (bend != BendDirection.none)
                {
                    r.Add(cur);
                }
            }

            return r;
        }
    }

    public static class OrthogonalConnector
    {
        private static readonly OrthogonalConnectorByproduct byproduct = new OrthogonalConnectorByproduct();

        public static List<Point> route(OrthogonalConnectorOpts opts)
        {
            var pointA = opts.pointA;
            var pointB = opts.pointB;
            var globalBoundsMargin = opts.globalBoundsMargin;
            List<Point> spots = new List<Point>();
            List<float> verticals = new();
            List<float> horizontals = new();
            var sideA = pointA.side;
            var sideB = pointB.side;
            var sideAVertical = UtilFuncs.isVerticalSide(sideA);
            var sideBVertical = UtilFuncs.isVerticalSide(sideB);
            var originA = UtilFuncs.computePt(pointA);
            var originB = UtilFuncs.computePt(pointB);
            var shapeA = Rectangle.fromRect(pointA.shape);
            var shapeB = Rectangle.fromRect(pointB.shape);
            var bigBounds = Rectangle.fromRect(opts.globalBounds);
            var shapeMargin = opts.shapeMargin;
            var inflatedA = shapeA.inflate(shapeMargin, shapeMargin);
            var inflatedB = shapeB.inflate(shapeMargin, shapeMargin);

            // Check bounding boxes collision
            if (inflatedA.intersects(inflatedB))
            {
                shapeMargin = 0;
                inflatedA = shapeA;
                inflatedB = shapeB;
            }

            var inflatedBounds = inflatedA.union(inflatedB).inflate(globalBoundsMargin, globalBoundsMargin);

            // Curated bounds to stick to
            var bounds = Rectangle.fromLTRB(
                Mathf.Max(inflatedBounds.left, bigBounds.left),
                Mathf.Max(inflatedBounds.top, bigBounds.top),
                Mathf.Min(inflatedBounds.right, bigBounds.right),
                Mathf.Min(inflatedBounds.bottom, bigBounds.bottom)
            );

            // Add edges to rulers
            foreach (var b in new[] { inflatedA, inflatedB })
            {
                verticals.Add(b.left);
                verticals.Add(b.right);
                horizontals.Add(b.top);
                horizontals.Add(b.bottom);
            }

            // Rulers at origins of shapes
            (sideAVertical ? verticals : horizontals).Add(sideAVertical ? originA.x : originA.y);
            (sideBVertical ? verticals : horizontals).Add(sideBVertical ? originB.x : originB.y);

            // Points of shape antennas
            foreach (var connectorPt in new[] { pointA, pointB })
            {
                var p = UtilFuncs.computePt(connectorPt);
                Action<float, float> add = (float dx, float dy) => spots.Add(UtilFuncs.makePt(p.x + dx, p.y + dy));

                switch (connectorPt.side)
                {
                    case Side.top:
                        add(0, -shapeMargin);
                        break;
                    case Side.right:
                        add(shapeMargin, 0);
                        break;
                    case Side.bottom:
                        add(0, shapeMargin);
                        break;
                    case Side.left:
                        add(-shapeMargin, 0);
                        break;
                }
            }

            //TODO 确认sort的正确性
            verticals.Sort((a, b) => a - b > 0f ? 1 : -1);
            horizontals.Sort((a, b) => a - b > 0f ? 1 : -1);

            // Create grid
            var grid = UtilFuncs.rulersToGrid(verticals, horizontals, bounds);
            var gridPoints = UtilFuncs.gridToSpots(grid, new[] { inflatedA, inflatedB });

            // Add to spots
            spots.AddRange(gridPoints);
            // spots.push(...gridPoints);

            // Create graph
            var tp = UtilFuncs.createGraph(spots);
            var graph = tp.Item1;
            var connections = tp.Item2;

            // Origin and destination by extruding antennas
            var origin = UtilFuncs.extrudeCp(pointA, shapeMargin);
            var destination = UtilFuncs.extrudeCp(pointB, shapeMargin);

            var start = UtilFuncs.computePt(pointA);
            var end = UtilFuncs.computePt(pointB);

            byproduct.spots = spots;
            byproduct.vRulers = verticals;
            byproduct.hRulers = horizontals;
            byproduct.grid = grid.rectangles();
            byproduct.connections = connections;

            var path = UtilFuncs.shortestPath(graph, origin, destination);

            if (path.Length > 0)
            {
                var allPoints = new List<Point>();
                allPoints.Add(start);
                allPoints.AddRange(UtilFuncs.shortestPath(graph, origin, destination));
                allPoints.Add(end);
                return UtilFuncs.simplifyPath(allPoints);
            }
            else
            {
                return new();
            }
        }
    }
}