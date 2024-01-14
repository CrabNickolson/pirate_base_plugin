using BepInEx;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PirateBase;

[Il2CppImplements(typeof(IModularContainer))]
public class ModModularContainer : Il2CppSystem.Object
{
    private Dictionary<long, Object> m_assetDict = new();
    private List<SceneAssetHandler.AssetDictEntry> m_assets = new();

    private static ModModularContainer s_instance;

    //

    public ModModularContainer(System.IntPtr ptr) : base(ptr) { }

    public ModModularContainer() : base(ClassInjector.DerivedConstructorPointer<ModModularContainer>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public static void Init()
    {
        if (s_instance != null)
            return;

        ClassInjector.RegisterTypeInIl2Cpp<ModModularContainer>();
        s_instance = new ModModularContainer();
        SceneModularHandler.registerStatic(s_instance.Cast<IModularContainer>());
    }

    public static T Load<T>(string _key) where T : Object
    {
        long id = stringToHash(_key);
        if (s_instance.m_assetDict.TryGetValue(id, out var asset) && asset != null)
        {
            return (T)asset;
        }
        else
        {
            var op = Addressables.LoadAssetAsync<T>(_key);
            op.WaitForCompletion();
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                registerAsset(id, op.Result);
                return op.Result;
            }
            else
            {
                return null;
            }
        }
    }

    public static T LoadComponent<T>(string _key) where T : Component
    {
        long id = stringToHash(_key + typeof(T).AssemblyQualifiedName);
        if (s_instance.m_assetDict.TryGetValue(id, out var asset) && asset != null)
        {
            return (T)asset;
        }
        else
        {
            var gameObject = Load<GameObject>(_key);
            if (gameObject != null)
            {
                var component = gameObject.GetComponent<T>();
                if (component != null)
                {
                    registerAsset(id, component);
                    return component;
                }
            }
            return null;
        }
    }

    // IModularContainer

    public string name => "pirate_base_modular";

    public Il2CppSystem.Collections.Generic.List<SceneAssetHandler.AssetDictEntry> getAssets()
    {
        return s_instance.m_assets.ToIL2CPP();
    }

    //

    private static void registerAsset(long _id, Object _asset)
    {
        var entry = new SceneAssetHandler.AssetDictEntry(_asset, _id);
        entry.m_obj = _asset;
        entry.m_lID = _id;

        s_instance.m_assets.Add(entry);
        s_instance.m_assetDict.Add(_id, _asset);
    }

    private static long stringToHash(string _input)
    {
        long h = 1125899906842597L;
        for (int i = 0; i < _input.Length; i++)
            h = 31 * h + _input[i];
        return h;
    }
}
