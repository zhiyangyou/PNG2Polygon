using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace AutoPolygon
{
    public static class DLLUnloader
    {
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        // 获取目标进程句柄
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        // 获取DLL模块句柄
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // 获取FreeLibrary函数地址
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        // 在目标进程中调用FreeLibrary
        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter,
            uint dwCreationFlags, IntPtr lpThreadId);

        // 关闭句柄
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        // 等待线程完成
        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        public static void UnloadDllByName(string name)
        {
            // 目标进程ID
            int processId = Process.GetCurrentProcess().Id;

            // 获取目标进程句柄
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                // 处理错误
                return;
            }

            // DLL模块名称
            string dllName = name;

            // 获取DLL模块句柄
            IntPtr hModule = GetModuleHandle(dllName);
            if (hModule == IntPtr.Zero)
            {
                // 处理错误
                CloseHandle(hProcess);
                return;
            }

            // 获取FreeLibrary函数地址
            IntPtr freeLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "FreeLibrary");
            if (freeLibraryAddr == IntPtr.Zero)
            {
                // 处理错误
                CloseHandle(hProcess);
                return;
            }

            // 在目标进程中调用FreeLibrary
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, freeLibraryAddr, hModule, 0, IntPtr.Zero);
            if (hThread == IntPtr.Zero)
            {
                // 处理错误
                CloseHandle(hProcess);
                return;
            }

            // 等待线程完成
            WaitForSingleObject(hThread, 0xFFFFFFFF);

            // 关闭句柄
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }
    }

    [InitializeOnLoad]
    public static class DLLAutoIniter
    {
        static DLLAutoIniter()
        {
            if (!DLLLoaderMenuItem.NeedAutoLoad())
            {
                return;
            }
            DLLLoaderMenuItem.InitAutoPolygonLibrary();
            AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnbeforeAssemblyReload;
        }

        private static void AssemblyReloadEventsOnbeforeAssemblyReload()
        {
            if (!DLLLoaderMenuItem.NeedAutoLoad())
            {  
                return;
            }
            DLLLoaderMenuItem.UnInitAutoPolygonLibrary();
            AssemblyReloadEvents.beforeAssemblyReload -= AssemblyReloadEventsOnbeforeAssemblyReload;
        }
    }

    public static class DLLLoaderMenuItem
    {
        private static string StrEnableKey = "IsEnableAutoPolygonDll";

        public static bool NeedAutoLoad()
        {
           return EditorPrefs.GetBool(StrEnableKey,false);
        }
        
        [MenuItem("Tools/AutoPolygon/开关-是否启用自动加载PolygonDll")]
        public static void EnableAutoPolygonDll()
        {
            var curIsEnable = EditorPrefs.GetBool(StrEnableKey,false);
            EditorPrefs.SetBool(StrEnableKey, !curIsEnable);
            Debug.Log(!curIsEnable ? "启用AutoPolygon自动加载" : "关闭AutoPolygon自动加载");
            if (!curIsEnable)
            {
                InitAutoPolygonLibrary();
            }
        }

        [MenuItem("Tools/AutoPolygon/OpenLib")]
        public static void InitAutoPolygonLibrary()
        {
            if (!Application.isPlaying)
            {
                unsafe
                {
                    IntPtr intPtr = DLLLoader.LoadDLL();
                    if (intPtr.ToPointer() == null)
                    {
                        Debug.LogError("加载Dll失败");
                    }
                    else
                    {
                        ExportFuncDef.Init();
                        Debug.Log($"加载Dll成功 ptr = {intPtr}");
                    }
                }
            }
        }

        [MenuItem("Tools/AutoPolygon/CloseLib")]
        public static void UnInitAutoPolygonLibrary()
        {
            if (!Application.isPlaying)
            {
                if (!DLLLoader.UnloadDLL())
                {
                    Debug.LogError("卸载Dll失败");
                }
                else
                {
                    ExportFuncDef.UnInit();
                    Debug.Log("卸载Dll成功");
                }
            }
        }
    }

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

        private static string DllName = "AutoPolygonDLL.dll";

        private static string DllPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dllPath))
                {
                    string dllFullPath = Application.dataPath + "";
                    DirectoryInfo dir = new DirectoryInfo(dllFullPath);
                    FileInfo fileInfo = new FileInfo(dir.Parent.FullName + $"/NativePlugin/win32/{DllName}");
                    _dllPath = fileInfo.FullName;
                }

                return _dllPath;
            }
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(string path);

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(IntPtr libraryHandle, string symbolName);

        public static IntPtr LoadDLL(string dynamicLibFullPath = null)
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

        public static bool UnloadDLL()
        {
            DLLUnloader.UnloadDllByName(DllName);
            _dllHandle = IntPtr.Zero;
            _hasLoaded = true;
            return true;
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