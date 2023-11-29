using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;


namespace App.Utils
{
    public static class DLLLoader
    {
#if UNITY_EDITOR_WIN

        private static string _dllPath = "";
        private static IntPtr _dllHandle;

        private static bool _hasLoaded = false;

        public static bool HasLoaded
        {
            get { return _hasLoaded; }
        }

        private static string DllPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dllPath))
                {
                    string dllFullPath = Application.dataPath + "";
                    DirectoryInfo dir = new DirectoryInfo(dllFullPath);
                    DirectoryInfo dirDllDir = new DirectoryInfo(dir.Parent.Parent.Parent.FullName + "/Plugin/win32");
                    FileInfo fileInfo = new FileInfo(dirDllDir.GetFiles().Where(info => info.FullName.EndsWith(".dll")).First().FullName);
                    _dllPath = fileInfo.FullName;
                }

                return _dllPath;
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        
        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(
            string path);

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(
            IntPtr libraryHandle,
            string symbolName);

        [DllImport("kernel32")]
        static extern bool FreeLibrary(
            IntPtr libraryHandle);

        public static IntPtr OpenLibrary(string dynamicLibFullPath = null)
        {
            if (string.IsNullOrEmpty(dynamicLibFullPath))
            {
                dynamicLibFullPath = DllPath;
            }

            _dllHandle = LoadLibrary(dynamicLibFullPath);
            if (_dllHandle == IntPtr.Zero)
            {
                throw new Exception("Couldn't open native library: " + dynamicLibFullPath);
            }

            _hasLoaded = true;

            return _dllHandle;
        }

        public static bool CloseLibrary()
        {
            var ret = FreeLibrary(_dllHandle);
            _hasLoaded = !ret;
            return ret;
        }

        public static T GetDelegate<T>(
            string functionName) where T : class
        {
            if (!_hasLoaded)
            {
                throw new Exception("Dll尚未加载");
            }

            IntPtr symbol = GetProcAddress(_dllHandle, functionName);
            if (symbol == IntPtr.Zero)
            {
                throw new Exception("Couldn't get function: " + functionName);
            }

            return Marshal.GetDelegateForFunctionPointer(symbol, typeof(T)) as T;
        }
#endif
    }
}