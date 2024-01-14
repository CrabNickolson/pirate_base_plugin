using BepInEx;
using System.Collections.Generic;

namespace PirateBase;

public static class ModThreadUtility
{
    [System.ThreadStatic]
    private static System.IntPtr s_threadPtr = System.IntPtr.Zero;
    private static List<System.IntPtr> s_attachedThreads = new();

    //

    public static System.IntPtr AttachThread()
    {
        if (s_threadPtr == System.IntPtr.Zero)
        {
            s_threadPtr = Il2CppInterop.Runtime.IL2CPP.il2cpp_thread_attach(Il2CppInterop.Runtime.IL2CPP.il2cpp_domain_get());
            s_attachedThreads.Add(s_threadPtr);
        }

        return s_threadPtr;
    }

    public static void DetachThread()
    {
        if (s_threadPtr != System.IntPtr.Zero)
        {
            Il2CppInterop.Runtime.IL2CPP.il2cpp_thread_detach(s_threadPtr);
            s_threadPtr = System.IntPtr.Zero;
        }
    }

    public static void DetachAllThreads()
    {
        foreach (var ptr in s_attachedThreads)
        {
            Il2CppInterop.Runtime.IL2CPP.il2cpp_thread_detach(ptr);
        }
        s_attachedThreads.Clear();
    }
}
