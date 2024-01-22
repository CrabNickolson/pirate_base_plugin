# Pirate Base

This is a [BepInEx](https://github.com/BepInEx/BepInEx) plugin for [Shadow Gambit](https://store.steampowered.com/app/1545560/Shadow_Gambit_The_Cursed_Crew/) that wraps core game functionality and makes it easier to access for other plugins.

## Usage (for Plugin Developers)
1. [Setup your BepInEx plugin project as you normally would](https://docs.bepinex.dev/master/articles/dev_guide/plugin_tutorial/1_setup.html).
2. [Download the latest Pirate Base Plugin release](https://github.com/CrabNickolson/pirate_base_plugin/releases/latest) or compile it yourself.
3. Place `PirateBasePlugin.dll` into your plugins source folder and add it as a dependency in the `project_name.csproj` file.
```xml
<Project Sdk="Microsoft.NET.Sdk">
    ...
    <ItemGroup>
        <Reference Include="PirateBasePlugin">
            <HintPath>path\to\PirateBasePlugin.dll</HintPath>
            <Private>False</Private>
        </Reference>
    </ItemGroup>
</Project>
```
4. Add it as a dependency of your plugin class by adding the `BepInDependency` attribute:
```csharp
//...
[BepInDependency(PirateBase.Plugin.c_pluginGUID)]
public class YourPlugin : BasePlugin
{
    //...
}
```

## IL2CPP
Shadow Gambit uses IL2CPP, which unfortunately makes modding fairly complicated. Here are some things to watch out for:
- Your cannot cast Il2CPP objects normally. Instead you must use the `x.Cast<T>()` method.
- If you directly override a IL2CPP method in a subclass, then you cannot call `base.method()` from within that method, as that would cause an infinite recursion.
- If you start a thread you must "attach" it by calling either `IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get())` or `ModThreadUtility.AttachThread()` from within the thread.
- Some IL2CPP structs have been converted into classes. Be careful not to initialize these with `default`.
    - This can be annoying as some game methods initialize affected struct parameters with `default`. You must set these to something else when calling these methods or the game may crash. `CharacterUtility` contains helper methods for some affected game methods.
- Some IL2CPP classes are not converted correctly and cannot be accessed directly from C#, e.g. `BalancedEnum`. You should still be able to access these classes via IL2CPP reflection.
- Calling IL2CPP methods can become very performance intensive in math heavy code. For this reason you should avoid Unity's math library as much as possible.
    - Use `System.Math` instead of `UnityEngine.Mathf`
    - Use `System.Numerics.Vector3` instead of `UnityEngine.Vector3`
    - There are extension methods to convert between the two `Vector3` classes: `x.ToNET()` and `x.ToIL2CPP()`
- [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki/Basic-usage) is very useful to work around broken IL2CPP stuff.

## Documentation

### Game Events
`GameEvents` is a helper class that let's you get callbacks for common game events. You will most likely need `GameEvents.RunOnGameInit(callback)` to be able to hook into game systems after they have been initialized.

### Scripting
`ModScripting` lets you register custom MiScript commands. To do this, you must create a new IL2CPP class that implements your commands:
```csharp
using Il2CppInterop.Runtime.Injection;
using PirateBase;

internal class YourScriptingClass : Il2CppSystem.Object
{
    public YourScriptingClass(System.IntPtr ptr) : base(ptr) { }

    public YourScriptingClass() : base(ClassInjector.DerivedConstructorPointer<YourScriptingClass>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    [ModScriptMethod("your-custom-method")]
    public string YourCustomMethod()
    {
        return "Hello World!";
    }

    [ModScriptMethod("your-other-custom-method")]
    public string YourOtherCustomMethod(string _name)
    {
        return $"Hello {_name}!";
    }
}
```

You must then register an instance of this class on game init with `ModScripting.RegisterLibrary(new YourScriptingClass());`

Then you can use your methods ingame in any MiScript field like this:
```
your-custom-method

-> Hello World!

your-other-custom-method Afia

-> Hello Afia!
```

There are some limitations:
- You cannot have multiple methods with the same command name.
- Your methods cannot be static.
- Your methods will not show up in `list-commands` or `help`.
- You cannot return `IEnumerator` from your methods.
- You cannot register custom properties or parsers.

### Saving
To create a custom component that can be saved ingame, create a new class that inherits from `ModSaveable`:

```csharp
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;
using PirateBase;

internal class YourSaveableClass : ModSaveable
{
    private float m_savedProperty;
    private MiCharacter m_savedReference;

    public YourSaveableClass(System.IntPtr ptr) : base(ptr) { }

    public YourSaveableClass() : base(ClassInjector.DerivedConstructorPointer<YourSaveableClass>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    [HideFromIl2Cpp]
    protected override void serializeMod(ref ModSerializeHelper _helper)
    {
        base.serializeMod(ref _helper);

        _helper.Serialize(0, m_savedProperty);
        _helper.SerializeUnityObject(1, m_savedReference);
    }

    [HideFromIl2Cpp]
    protected override void deserializeMod(ref ModDeserializeHelper _helper)
    {
        base.deserializeMod(ref _helper);

        _helper.Deserialize(0, ref m_savedProperty);
        _helper.DeserializeUnityObject(1, ref m_savedReference);
    }
}
```

Override `serializeMod()` and `deserializeMod()` and add the fields that you want to have saved. Be careful that you use the correct serialize/deserialize methods for your field type. Each field needs to have a (for the type) unique ID assigned to it.

When your plugin is loading you must register your component class with `ClassInjector.RegisterTypeInIl2Cpp<YourSaveableClass>()` and `ModSaveManager.RegisterType<YourSaveableClass>()`.

When you instantiate your component, it must be parented under a save root:
```csharp
var go = new GameObject();
go.transform.SetParent(SaveLoadSceneManager.transGetRoot());
go.AddComponent<YourSaveableClass>();
````

#### Asset References
Any assets (e.g. prefabs/materials/etc.) that are instantiated in a saveroot or are referenced in saved components must be loaded on game init with `ModModularContainer.Load<T>("addressable_key")`, so that the savesystem knows about them. Unfortunately you also need to manually load any dependencies (e.g. materials on prefab models) of these assets. If this becomes too much work, you could also consider working around this by keeping any models/particle effects/etc. outside of the saveroot and then manually create and destroy these objects when the saved component is created and destroyed.

#### Limitations
- It is (probably) not possible to save custom modded classes/structs (except for classes that inherit from UnityEngine.Object).
- It is not possible to save coroutines/`IEnumerator`.
- It is not possible to save custom delegates.
- It is also not possible to save callbacks you registered on existing saved game classes, e.g. `MiCharacter.m_evOnDeath`.
    - You need to remove your callbacks on `GameEvents.beforeSave` and then reregister them on `GameEvents.afterSave`.
- `ModSerializeHelper` currently does not expose every saveable struct type, e.g. `double` or `long`.
    - You can still save these fields by adding them manually to `_helper.dictFields`.

### Update
With `ModUpdate.shouldSkipUpdate` you can check if it is currently safe to execute Update methods on your custom components.
```csharp
private void Update()
{
    if (ModUpdate.shouldSkipUpdate)
        return;

    // do stuff
}
```

### Shader
For some reason `UnityEngine.Shader.Find("name")` does not work. Instead you can directly load shaders with the Addressable system, although for most objects you will want you will want to use the "MiStandardMetallic" shaders.
You can access these with `ShaderUtility.FindStandardHideVCShader()` and `ShaderUtility.FindStandardShowVCShader()`, which will make the viewcone draw behind or infront of your object respectively.

### Asset Bundle Checks
Shadow Gambit checks the file size of its own asset bundles on boot, and refuses to start if they don't match a known value. This makes it difficult to modify these bundles.

You can disable this check either by deleting `[game folder]\ShadowGambit_TCC_Data\StreamingAssets\aa\catalog_assetbundle_filesize_cache.json` or by calling `ModAddressableManager.disableBundleFileSizeChecks = true` from within the load function of your plugin.
