using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AutoPolygon
{
    public enum EAutoPolygonDataType
    {
        V3F_C4B_T2F = 0,
        INDEX = 1,
    } 


    public static class ExportFuncDef
    {
        private static bool _hasInit = false;

        public delegate void CppDele_AutoPolygonGenerate_OverdrawInfoCallback(float triArea, int alphaThresholdPixelArea, float fullArea);

        public delegate void CppDele_AutoPolygonGenerate_DataCallback(IntPtr data, int dataLen, int dataType);

        public delegate int CppDele_AutoPolygonGenerateByColors(
            IntPtr data,
            int dataLen,
            int width,
            int height,
            IntPtr name,
            int rect_left,
            int rect_bottom,
            int rect_width,
            int rect_height,
            float epsilon,
            float alpha_threshold,
            IntPtr callback,
            IntPtr overdrawInfoCallback
        );

        public delegate int CppDele_AutoPolygonGenerateByPngFile(
            IntPtr png32FileNameFullPath,
            int rect_left,
            int rect_bottom,
            int rect_width,
            int rect_height,
            float epsilon,
            float alpha_threshold,
            IntPtr callback,
            IntPtr overdrawInfoCallback
        );

        private static CppDele_AutoPolygonGenerateByColors _cppDeleAutoPolygonGenerateByColorsByColor = null;

        private static CppDele_AutoPolygonGenerateByColors CppDeleAutoPolygonGenerateByColorsByColor
        {
            get
            {
                if (!_hasInit)
                {
                    return null;
                }

                return _cppDeleAutoPolygonGenerateByColorsByColor;
            }
        }


        private static CppDele_AutoPolygonGenerateByPngFile _cppDele_AutoPolygonGenerateByPNGFile = null;

        private static CppDele_AutoPolygonGenerateByPngFile CPPDeleAutoPolygonGenerateByPNGFile
        {
            get
            {
                if (!_hasInit)
                {
                    return null;
                }

                return _cppDele_AutoPolygonGenerateByPNGFile;
            }
        }


        unsafe static void CopyDataFormCpp(ref V3F_C4B_T2F[] verts, ref ushort[] indices, int type, int len, IntPtr data)
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
        }

        static void FillData(V3F_C4B_T2F[] verts, ushort[] indices, List<Vector3> listVerts, List<Vector2> listUVs, List<ushort> listIndex)
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


        public static int AutoPolygonGenerateByPNGFullPath(
            string pngFullPath,
            float epsilon,
            float alpha_thresold,
            List<Vector3> listVerts,
            List<Vector2> listUVs,
            List<ushort> listIndex,
            ref Overdrawinfo info
        )
        {
            if (_cppDeleAutoPolygonGenerateByColorsByColor == null)
            {
                Debug.LogError("运行库已经卸载，请重新加载");
                return -1;
            }

            V3F_C4B_T2F[] verts = null;
            ushort[] indices = null;
            CppDele_AutoPolygonGenerate_DataCallback callback = (data, len, type) => { CopyDataFormCpp(ref verts, ref indices, type, len, data); };
            float outTriangleArea = 0;
            int outAlphaThresholdPixelArea = 0;
            float outfullArea = 0;
            CppDele_AutoPolygonGenerate_OverdrawInfoCallback infoCallback = (_triangleArea, _alphaThresholdPixelArea, _fullArea) =>
            {
                outTriangleArea = _triangleArea;
                outAlphaThresholdPixelArea = _alphaThresholdPixelArea;
                outfullArea = _fullArea;
            };
            var ret = _cppDele_AutoPolygonGenerateByPNGFile(
                Marshal.StringToHGlobalAnsi(pngFullPath),
                0, 0, 0, 0,
                epsilon, alpha_thresold,
                Marshal.GetFunctionPointerForDelegate(callback),
                Marshal.GetFunctionPointerForDelegate(infoCallback)
            );
            info.TriArea = outTriangleArea;
            info.pixelArea = outAlphaThresholdPixelArea;
            info.totalArea = outfullArea;
            if (ret != 0)
            {
                Debug.LogError($" 生成网格失败ret is {ret}");
            }
            else
            {
                FillData(verts, indices, listVerts, listUVs, listIndex);
            }

            return ret;
        }

        /*
        public static int AutoPolygonGenerateByColors(
            Color32[] cols,
            int width,
            int height,
            string name,
            float epsilon,
            float alpha_thresold,
            List<Vector3> listVerts,
            List<Vector2> listUVs,
            List<ushort> listIndex
        )
        {
            if (_cppDeleAutoPolygonGenerateByColorsByColor == null)
            {
                Debug.LogError("运行库已经卸载，请重新加载");
                return -1;
            }

            unsafe
            {
                V3F_C4B_T2F[] verts = null;
                ushort[] indices = null;
                fixed (Color32* colData = cols)
                {
                    CppDele_AutoPolygonGenerate_DataCallback callback = (data, len, type) => { CopyDataFormCpp(ref verts, ref indices, type, len, data); };
                    IntPtr dataPtr = new IntPtr(colData);
                    int dataLen = cols.Length * 4;
                    int w = width;
                    int h = height;
                    var ret = _cppDeleAutoPolygonGenerateByColorsByColor(
                        dataPtr,
                        dataLen,
                        w, h,
                        Marshal.StringToHGlobalAnsi(name),
                        0, 0, w, h,
                        epsilon, alpha_thresold,
                        Marshal.GetFunctionPointerForDelegate(callback));
                    if (ret != 0)
                    {
                        Debug.LogError($" 生成网格失败ret is {ret}");
                    }
                    else
                    {
                        GeometryUtils.FillData(verts, indices, listVerts, listUVs, listIndex);
                        GeometryUtils.InverseXY_Y(listVerts, height);
                    }

                    return ret;
                }
            }
        }
*/
        public static void UnInit()
        {
            _cppDeleAutoPolygonGenerateByColorsByColor = null;
            _cppDele_AutoPolygonGenerateByPNGFile = null;
            _hasInit = false;
        }

        public static bool Init()
        {
            _cppDeleAutoPolygonGenerateByColorsByColor = DLLLoader.GetDelegate<CppDele_AutoPolygonGenerateByColors>("AutoPolygonGenerate");
            _cppDele_AutoPolygonGenerateByPNGFile = DLLLoader.GetDelegate<CppDele_AutoPolygonGenerateByPngFile>("AutoPolygonGenerateByPngFile");
            _hasInit = _cppDeleAutoPolygonGenerateByColorsByColor != null;
            return _hasInit;
        }
    }
}