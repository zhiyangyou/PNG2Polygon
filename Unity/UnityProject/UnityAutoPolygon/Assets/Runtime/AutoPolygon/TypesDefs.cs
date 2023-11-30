using System.Runtime.InteropServices;

namespace Runtime.AutoPolygon
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec3
    {
        [FieldOffset(0)] public float x;
        [FieldOffset(4)] public float y;
        [FieldOffset(8)] public float z;

        public Vec3(float z, float y, float x)
        {
            this.z = z;
            this.y = y;
            this.x = x;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Color4B
    {
        [FieldOffset(0)] public byte r;
        [FieldOffset(1)] public byte g;
        [FieldOffset(2)] public byte b;
        [FieldOffset(3)] public byte a;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Tex2F
    {
        [FieldOffset(0)] public float u;
        [FieldOffset(4)] public float v;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct V3F_C4B_T2F
    {
        [FieldOffset(0)] public Vec3 vertices;

        /// colors (4B)
        [FieldOffset(12)]
        public Color4B colors;

        // tex coords (2F)
        [FieldOffset(16)] public Tex2F texCoords;
    }
}