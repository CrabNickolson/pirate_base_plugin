using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace PirateBase;

public class ModUpdate : MonoBehaviour
{
    private static bool s_shouldSkipUpdate = true;
    public static bool shouldSkipUpdate => s_shouldSkipUpdate;

    private void Update()
    {
        s_shouldSkipUpdate = SaveLoadSceneManager.bProcessing || !MiSceneManager.bSceneInitialized || !MiAwakeSystemUtility.bAwakeDoneForAllScenes();
    }
}
