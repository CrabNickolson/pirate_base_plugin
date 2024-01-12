using BepInEx;
using Il2CppSystem.Collections.Generic;

namespace PirateBase;

public static class ModSaveManager
{
    private static List<LoadDataStructure> s_lastLoadDataStructures;
    private static System.Collections.Generic.Dictionary<long, LoadDataStructure> s_lastIDToDataStructureDict;
    private static nint s_lastLoadDataStructuresPtr;

    private static bool s_dictsAreSet;
    private static Dictionary<long, UnityEngine.Object> s_lastIdToObjectDict;
    private static Dictionary<long, UnityEngine.Object> s_lastIdToAssetDict;
    private static Dictionary<long, Il2CppSystem.Object> s_lastIdToClassDict;

    private static System.Collections.Generic.List<ModSaveable> s_objsToSave;

    public static void Init()
    {
        s_objsToSave = new();
        s_lastIDToDataStructureDict = new();
        s_lastIdToObjectDict = new();
        s_lastIdToAssetDict = new();
        s_lastIdToClassDict = new();

        RegisterType<ModSaveable>();

        GameEvents.beforeSave += onBeforeSave;
        GameEvents.afterSaveBeforeWrite += onAfterSaveBeforeWrite;
        GameEvents.afterSave += onAfterSave;
        GameEvents.deserializeSaveGame += onDeserializeSaveGame;
        GameEvents.beforeLoad += onBeforeLoad;
    }

    public static void Dispose()
    {
        s_objsToSave = null;
        s_lastIDToDataStructureDict = null;
        s_lastIdToObjectDict = null;
        s_lastIdToAssetDict = null;
        s_lastIdToClassDict = null;

        if (s_lastLoadDataStructuresPtr != 0)
        {
            Il2CppInterop.Runtime.IL2CPP.il2cpp_gchandle_free(s_lastLoadDataStructuresPtr);
            s_lastLoadDataStructuresPtr = 0;
        }

        GameEvents.beforeSave -= onBeforeSave;
        GameEvents.afterSaveBeforeWrite -= onAfterSaveBeforeWrite;
        GameEvents.afterSave -= onAfterSave;
        GameEvents.deserializeSaveGame -= onDeserializeSaveGame;
        GameEvents.beforeLoad -= onBeforeLoad;
    }

    public static void RegisterObjectToSave(ModSaveable _modSaveable)
    {
        s_objsToSave.Add(_modSaveable);
    }

    public static void RegisterType<T>()
    {
        // injected types for some reason cannot be found with il2cpps Type.GetType(), so we need to hack them into the savesystems ReflectionHelper.
        var type = Il2CppInterop.Runtime.Il2CppType.Of<T>();
        ReflectionHelper.s_dicCachedTypes.TryAdd(ReflectionHelper.StringFromType(type), type);
    }

    public static List<LoadDataStructure> GetLoadDataStructuresForSave()
    {
        return SaveProcessManager.instance.m_loadDataContainerMemory.m_lLoadDataStructure;
    }

    public static List<LoadDataStructure> GetLoadDataStructuresForLoad()
    {
        if (SaveProcessManager.s_bLoadingSaveFileFromMemory)
            return SaveProcessManager.instance.m_loadDataContainerMemory.m_lLoadDataStructure;

        return s_lastLoadDataStructures;
    }

    public static LoadDataStructure GetDataStructure(long _id)
    {
        return s_lastIDToDataStructureDict[_id];
    }

    public static Dictionary<long, UnityEngine.Object> GetObjectDict()
    {
        if (!s_dictsAreSet)
            updateIDDicts();
        return s_lastIdToObjectDict;
    }

    public static Dictionary<long, UnityEngine.Object> GetAssetDict()
    {
        if (!s_dictsAreSet)
            updateIDDicts();
        return s_lastIdToAssetDict;
    }

    public static Dictionary<long, Il2CppSystem.Object> GetClassDict()
    {
        if (!s_dictsAreSet)
            updateIDDicts();
        return s_lastIdToClassDict;
    }

    private static void onBeforeSave(SaveGameHolder _saveGameHolder)
    {
        s_objsToSave.Clear();
    }

    private static void onAfterSaveBeforeWrite()
    {
        updateIDToDataStructureDict(GetLoadDataStructuresForSave());
        foreach (var obj in s_objsToSave)
        {
            obj.OnSerializeMod();
        }
    }

    private static void onAfterSave(SaveGameHolder _saveGameHolder, SaveProcessManager.SaveProcessResult _result)
    {
        s_objsToSave.Clear();
    }

    private static void onDeserializeSaveGame(SaveGameHolder _saveGameHolder)
    {
        var list = _saveGameHolder.Data.lLoadDataStructure;

        if (s_lastLoadDataStructuresPtr != 0)
            Il2CppInterop.Runtime.IL2CPP.il2cpp_gchandle_free(ModSaveManager.s_lastLoadDataStructuresPtr);
        s_lastLoadDataStructuresPtr = Il2CppInterop.Runtime.IL2CPP.il2cpp_gchandle_new(list.Pointer, false);

        s_lastLoadDataStructures = list;
    }

    private static void onBeforeLoad(SaveGameHolder _saveGameHolder)
    {
        updateIDToDataStructureDict(GetLoadDataStructuresForLoad());
        clearIDDicts();
    }

    private static void updateIDToDataStructureDict(List<LoadDataStructure> _loadDataStructures)
    {
        s_lastIDToDataStructureDict.Clear();
        foreach (var loadDataStructure in _loadDataStructures)
            s_lastIDToDataStructureDict.TryAdd(loadDataStructure.m_lID, loadDataStructure);
    }

    private static void clearIDDicts()
    {
        s_lastIdToObjectDict.Clear();
        s_lastIdToAssetDict.Clear();
        s_lastIdToClassDict.Clear();
        s_dictsAreSet = false;
    }

    private static void updateIDDicts()
    {
        clearIDDicts();

        foreach (var entry in SaveLoadIDManager.dIDs)
            s_lastIdToObjectDict.Add(entry.Value.m_lID, entry.Key);

        foreach (var entry in SaveLoadIDManager.dAssets)
            s_lastIdToAssetDict.Add(entry.Value.m_lID, entry.Key);

        foreach (var entry in SaveLoadIDManager.s_dClassIDs)
            s_lastIdToClassDict.Add(entry.Value.m_lID, entry.Key);

        s_dictsAreSet = true;
    }
}
