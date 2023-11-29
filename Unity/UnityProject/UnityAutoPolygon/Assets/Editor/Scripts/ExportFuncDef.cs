using System;
using System.Runtime.InteropServices;

namespace App.Utils
{
    enum EAutoPolygonDataType
    {
        V3F_C4B_T2F = 0,
        INDEX = 1,
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Vec3
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
    struct Color4B
    {
        [FieldOffset(0)] public byte r;
        [FieldOffset(1)] public byte g;
        [FieldOffset(2)] public byte b;
        [FieldOffset(3)] public byte a;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct Tex2F
    {
        [FieldOffset(0)] public float u;
        [FieldOffset(4)] public float v;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct V3F_C4B_T2F
    {
        [FieldOffset(0)] public Vec3 vertices;

        /// colors (4B)
        [FieldOffset(12)]
        public Color4B colors;

        // tex coords (2F)
        [FieldOffset(16)] public Tex2F texCoords;
    }

    public static class ExportFuncDef
    {
        public delegate void CppDele_AutoPolygonGenerate_DataCallback(IntPtr data, int dataLen, int dataType);

        public delegate int CppDele_AutoPolygonGenerate(
            IntPtr data,
            int dataLen,
            int width,
            int height,
            IntPtr name,
            float rect_left,
            float rect_bottom,
            float rect_width,
            float rect_height,
            float epsilon,
            float alpha_threshold,
            IntPtr callback
        );

        public static CppDele_AutoPolygonGenerate _cppDele_AutoPolygonGenerate = null;

        public static void UnInit()
        {
            _cppDele_AutoPolygonGenerate = null;
        }

        public static void Init()
        {
            ExportFuncDef._cppDele_AutoPolygonGenerate = DLLLoader.GetDelegate<ExportFuncDef.CppDele_AutoPolygonGenerate>("AutoPolygonGenerate");
        }
    }
}