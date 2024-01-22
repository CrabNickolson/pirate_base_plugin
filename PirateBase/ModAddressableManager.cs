using BepInEx;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace PirateBase;

public static class ModAddressableManager
{
    private static Harmony s_harmony;

    private static bool s_disableBundleFileSizeChecks;
    public static bool disableBundleFileSizeChecks => s_disableBundleFileSizeChecks;

    //

    public static void Init()
    {
        s_harmony = Harmony.CreateAndPatchAll(typeof(AddressablePatches));

        if (System.Environment.GetCommandLineArgs().Contains("-disable-bundle-file-size-checks"))
            s_disableBundleFileSizeChecks = true;
    }

    public static void Dispose()
    {
    }

    //

    private class AddressablePatches
    {
        [HarmonyPatch(typeof(MiAddressableHandler), nameof(MiAddressableHandler.verifyAssetBundleFileSize))]
        [HarmonyPostfix]
        public static void patchVerifyAssetBundleFileSize(ref bool __result, string _assetBundleFullPath)
        {
            if (s_disableBundleFileSizeChecks && !__result)
            {
                Plugin.PluginLog.LogWarning($"Bundle File Size check failed for {_assetBundleFullPath}.");
                __result = true;
            }
        }
    }
}