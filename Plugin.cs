using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;

namespace PirateBase;

[BepInPlugin(c_pluginGUID, c_pluginName, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess(c_processName)]
public class Plugin : BasePlugin
{
    internal static ManualLogSource PluginLog { get; private set; }

    public const string c_pluginGUID = "com.crabnickolson.pirate_base";
    public const string c_pluginName = "PirateBase";
    public const string c_processName = "ShadowGambit_TCC.exe";

    //

    public override void Load()
    {
        PluginLog = Log;

        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<ModSaveable>())
            ClassInjector.RegisterTypeInIl2Cpp<ModSaveable>();
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<ModModularContainer>())
            ClassInjector.RegisterTypeInIl2Cpp<ModModularContainer>();

        ModAddressableManager.Init();
        GameEvents.Init();
        GameEvents.RunOnGameInit(onGameInit);
        GameEvents.applicationQuit += onApplicationQuit;

        Log.LogInfo($"Plugin {c_pluginGUID} is loaded!");
    }

    public override bool Unload()
    {
        ModAddressableManager.Dispose();
        GameEvents.Dispose();
        ModSaveManager.Dispose();
        ModThreadUtility.DetachAllThreads();

        Log.LogInfo($"Plugin {c_pluginGUID} is unloaded!");

        return base.Unload();
    }

    private void onGameInit()
    {
        AddComponent<ModUpdate>();
        ModModularContainer.Init();
        ModSaveManager.Init();
        ModScripting.Init();

        Log.LogInfo($"Plugin {c_pluginGUID} game init!");
    }

    private void onApplicationQuit()
    {
        ModThreadUtility.DetachAllThreads();
        PluginLog.LogInfo("onApplicationQuit");
    }
}
