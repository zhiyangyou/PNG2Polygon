using System;
using UnityEditor;
using UnityEngine;

namespace App.Utils
{
    public static class DLLLoaderMenuItem
    {
        [MenuItem("YzyTools/OpenLib")]
        static void OpenDynamicLib()
        {
            unsafe
            {
                IntPtr intPtr = DLLLoader.OpenLibrary();
                if (intPtr.ToPointer() == null)
                {
                    Debug.LogError("加载Dll失败");
                }
                else
                {
                    Debug.Log("加载Dll成功");
                }
            }
        }

        [MenuItem("YzyTools/CloseLib")]
        static void CloseDynamicLib()
        {
            if (!DLLLoader.CloseLibrary())
            {
                Debug.LogError("卸载Dll失败");
            }
            else
            {
                Debug.LogError("卸载Dll成功");
            }
        }
    }
}