using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using App.Utils.Utils;
using Runtime.AutoPolygon;
using UnityEngine;

namespace App.Utils
{
    public enum EAutoPolygonDataType
    {
        V3F_C4B_T2F = 0,
        INDEX = 1,
    }


    public static class ExportFuncDef
    {
        private static bool _hasInit = false;

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

        private static CppDele_AutoPolygonGenerate _cppDele_AutoPolygonGenerate = null;

        public static CppDele_AutoPolygonGenerate cppDele_AutoPolygonGenerate
        {
            get
            {
                if (!_hasInit)
                {
                    return null;
                }

                return _cppDele_AutoPolygonGenerate;
            }
        }


        public static int AutoPolygonGenerat(
            Color32[] cols,
            int width,
            int height,
            string name,
            float epsilon,
            float alpha_thresold,
            List<Vector3> listVerts,
            List<Vector2> listUVs,
            List<ushort> listIndex,
            bool needInverseTriangles = true
        )
        {
            unsafe
            {
                V3F_C4B_T2F[] verts = null;
                ushort[] indices = null;
                fixed (Color32* colData = cols)
                {
                    CppDele_AutoPolygonGenerate_DataCallback callback = (data, len, type) =>
                    {
                        EAutoPolygonDataType eAutoPolygonDataType = (EAutoPolygonDataType)type;
                        switch (eAutoPolygonDataType)
                        {
                            case EAutoPolygonDataType.V3F_C4B_T2F:
                                verts = new V3F_C4B_T2F[len / sizeof(V3F_C4B_T2F)];
                                fixed (V3F_C4B_T2F* dataDst = verts)
                                {
                                    DLLLoader.CopyMemory(new IntPtr(dataDst), data, (uint)len);
                                }

                                break;
                            case EAutoPolygonDataType.INDEX:
                                indices = new ushort[len / sizeof(ushort)];
                                fixed (ushort* dataDst = indices)
                                {
                                    DLLLoader.CopyMemory(new IntPtr(dataDst), data, (uint)len);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    };
                    IntPtr dataPtr = new IntPtr(colData);
                    int dataLen = cols.Length * 4;
                    int w = width;
                    int h = height;
                    var ret = _cppDele_AutoPolygonGenerate(
                        dataPtr,
                        dataLen,
                        w, h,
                        Marshal.StringToHGlobalAnsi(name),
                        0f, 0f, w, h,
                        epsilon, alpha_thresold,
                        Marshal.GetFunctionPointerForDelegate(callback));
                    if (ret != 0)
                    {
                        Debug.LogError($" 生成网格失败ret is {ret}");
                    }
                    else
                    {
                        GeometryUtils.FillData(verts, indices, listVerts, listUVs, listIndex);
                        if (needInverseTriangles)
                        {
                            // GeometryUtils.InverseTriangle(listIndex);
                        }
                        // GeometryUtils.InverseUV_V(listUVs);
                        GeometryUtils.InverseXY_Y(listVerts,height);
                    }

                    return ret;
                }
            }
        }

        public static void UnInit()
        {
            _cppDele_AutoPolygonGenerate = null;
            _hasInit = false;
        }

        public static void Init()
        {
            _cppDele_AutoPolygonGenerate = DLLLoader.GetDelegate<CppDele_AutoPolygonGenerate>("AutoPolygonGenerate");
            _hasInit = true;
        }
    }
}