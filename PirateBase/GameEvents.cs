using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace PirateBase;

public static class GameEvents
{
    private static Harmony s_harmony;

    private static GameEventHelper s_helperInstance;
    private static System.Action s_gameInitCallbacks;

    private static Il2CppSystem.Action s_onSceneInitFinished;
    public static System.Action sceneInitFinished { get; set; }

    public static System.Action applicationQuit { get; set; }

    private static SaveProcessManager.BeforeSaveDelegate s_onBeforeSave;
    private static SaveProcessManager.AfterSaveDelegate s_onAfterSave;
    private static SaveProcessManager.BeforeLoadDelegate s_onBeforeLoad;
    private static SaveProcessManager.AfterLoadDelegate s_onAfterLoad;

    public static System.Action<SaveGameHolder> beforeSave { get; set; }
    public static System.Action afterSaveBeforeWrite { get; set; }
    public static System.Action<SaveGameHolder, SaveProcessManager.SaveProcessResult> afterSave { get; set; }
    public static System.Action<SaveGameHolder> deserializeSaveGame { get; set; }
    public static System.Action<SaveGameHolder> beforeLoad { get; set; }
    public static System.Action<SaveGameHolder, SaveProcessManager.LoadProcessResult> afterLoad { get; set; }

    public static System.Action startModEditing { get; set; }
    public static System.Action stopModEditing { get; set; }


    public static void Init()
    {
        s_harmony = Harmony.CreateAndPatchAll(typeof(EventPatches));

        if (s_helperInstance == null)
        {
            s_helperInstance = IL2CPPChainloader.AddUnityComponent<GameEventHelper>();
            s_helperInstance.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(waitForGameInitCoro()));
        }

        RunOnGameInit(() =>
        {
            s_onBeforeSave = (SaveProcessManager.BeforeSaveDelegate)onBeforeSave;
            SaveProcessManager.instance.add_evOnBeforeSave(s_onBeforeSave);
            s_onAfterSave = (SaveProcessManager.AfterSaveDelegate)onAfterSave;
            SaveProcessManager.instance.add_evOnAfterSave(s_onAfterSave);
            s_onBeforeLoad = (SaveProcessManager.BeforeLoadDelegate)onBeforeLoad;
            SaveProcessManager.instance.add_evOnBeforeLoad(s_onBeforeLoad);
            s_onAfterLoad = (SaveProcessManager.AfterLoadDelegate)onAfterLoad;
            SaveProcessManager.instance.add_evOnAfterLoad(s_onAfterLoad);

            s_onSceneInitFinished = (Il2CppSystem.Action)onSceneInitFinished;
            MiSceneLoaderTactics.s_evOnSceneInitFinished += s_onSceneInitFinished;
        });
    }

    public static void Dispose()
    {
        if (s_helperInstance != null)
        {
            Object.Destroy(s_helperInstance);
            s_helperInstance = null;
        }

        if (SaveProcessManager.bInstanceExists)
        {
            if (s_onBeforeSave != null)
            {
                SaveProcessManager.instance.remove_evOnBeforeSave(s_onBeforeSave);
                s_onBeforeSave = null;
            }
            if (s_onAfterSave != null)
            {
                SaveProcessManager.instance.remove_evOnAfterSave(s_onAfterSave);
                s_onAfterSave = null;
            }
            if (s_onBeforeLoad != null)
            {
                SaveProcessManager.instance.remove_evOnBeforeLoad(s_onBeforeLoad);
                s_onBeforeLoad = null;
            }
            if (s_onAfterLoad != null)
            {
                SaveProcessManager.instance.remove_evOnAfterLoad(s_onAfterLoad);
                s_onAfterLoad = null;
            }
        }

        if (s_onSceneInitFinished != null)
        {
            s_onSceneInitFinished -= (Il2CppSystem.Action)onSceneInitFinished;
            s_onSceneInitFinished = null;
        }
    }

    //

    public static void RunOnGameInit(System.Action _callback)
    {
        if (MiInitialization.bFinished)
            _callback?.Invoke();
        else
            s_gameInitCallbacks += _callback;
    }

    private static System.Collections.IEnumerator waitForGameInitCoro()
    {
        while (!MiInitialization.bFinished)
            yield return null;

        s_gameInitCallbacks?.Invoke();
        s_gameInitCallbacks = null;
    }

    public static void RunNextUpdate(System.Action _callback, int _updateCount = 1)
    {
        s_helperInstance.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(waitForNextUpdateCoro(_callback, _updateCount)));
    }

    public static void RunNextFixedUpdate(System.Action _callback, int _updateCount = 1)
    {
        s_helperInstance.StartCoroutine(BepInEx.Unity.IL2CPP.Utils.Collections.CollectionExtensions.WrapToIl2Cpp(waitForNextFixedUpdateCoro(_callback, _updateCount)));
    }

    private static System.Collections.IEnumerator waitForNextUpdateCoro(System.Action _callback, int _updateCount)
    {
        for (int i = 0; i < _updateCount; i++)
        {
            yield return null;
        }
        _callback?.Invoke();
    }

    private static System.Collections.IEnumerator waitForNextFixedUpdateCoro(System.Action _callback, int _updateCount)
    {
        for (int i = 0; i < _updateCount; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        _callback?.Invoke();
    }

    private class GameEventHelper : MonoBehaviour
    {
        private void OnApplicationQuit()
        {
            GameEvents.onApplicationQuit();
        }
    }

    //

    private static void onBeforeSave(SaveGameHolder _saveGameHolder)
    {
        beforeSave?.Invoke(_saveGameHolder);
    }

    private static void onAfterSaveBeforeWrite()
    {
        afterSaveBeforeWrite?.Invoke();
    }

    private static void onAfterSave(SaveGameHolder _saveGameHolder, SaveProcessManager.SaveProcessResult _result)
    {
        afterSave?.Invoke(_saveGameHolder, _result);
    }

    private static void onDeserializeSaveGame(SaveGameHolder _saveGameHolder)
    {
        deserializeSaveGame?.Invoke(_saveGameHolder);
    }

    private static void onBeforeLoad(SaveGameHolder _saveGameHolder)
    {
        beforeLoad?.Invoke(_saveGameHolder);
    }

    private static void onAfterLoad(SaveGameHolder _saveGameHolder, SaveProcessManager.LoadProcessResult _result)
    {
        afterLoad?.Invoke(_saveGameHolder, _result);
    }

    private static void onSceneInitFinished()
    {
        sceneInitFinished?.Invoke();
    }

    private static void onStartModEditing()
    {
        startModEditing?.Invoke();
    }

    private static void onStopModEditing()
    {
        stopModEditing?.Invoke();
    }

    private static void onApplicationQuit()
    {
        applicationQuit?.Invoke();
    }

    private class EventPatches
    {
        [HarmonyPatch(typeof(MiCoreServices.SaveHolderSingleFile<SaveGameHeader, SaveGame>), nameof(MiCoreServices.SaveHolderSingleFile<SaveGameHeader, SaveGame>.OnLoadFinished))]
        [HarmonyPrefix]
        public static bool patchOnLoadFinished(MiCoreServices.SaveHolderSingleFile<SaveGameHeader, SaveGame> __instance, ref MiCoreServices.StorageOperation.AccessState _eResult)
        {
            if (_eResult == MiCoreServices.StorageOperation.AccessState.Success)
            {
                var saveGameHolder = __instance.TryCast<SaveGameHolder>();
                if (saveGameHolder != null)
                {
                    GameEvents.onDeserializeSaveGame(saveGameHolder);
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(SaveLoadSceneManager), nameof(SaveLoadSceneManager.save))]
        [HarmonyPostfix]
        public static void patchSave(LoadDataContainer _loadContainer)
        {
            if (SaveLoadSceneManager.bFailed)
                return;

            GameEvents.onAfterSaveBeforeWrite();
        }

        [HarmonyPatch(typeof(MiModdingHandler), nameof(MiModdingHandler.startEditing))]
        [HarmonyPostfix]
        public static void patchStartEditing()
        {
            GameEvents.onStartModEditing();
        }

        [HarmonyPatch(typeof(MiModdingHandler), nameof(MiModdingHandler.stopEditing))]
        [HarmonyPostfix]
        public static void patchStopEditing()
        {
            GameEvents.onStopModEditing();
        }
    }
}
