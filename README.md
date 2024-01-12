# PirateBase

A [BepInEx](https://github.com/BepInEx/BepInEx) plugin for Shadow Gambit that wraps core game functionality and makes it easier to access for other plugins.

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
- Your cannot cast Il2CPP objects normally. Instead you must use the `Cast<T>()` method.
- If you directly override a IL2CPP method in a subclass, then you cannot call `base.method()` from within that method, as that would cause an infinite recursion.
- If you start a thread you must "attach" it by calling either `IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get())` or `ModThreadUtility.AttachThread()` from within the thread.
- Some IL2CPP structs have been converted into classes. Be careful not to initialize these with `default`.
    - This can be annoying as some game methods initialize affected struct parameters with `default`. You must set these to something else when calling these methods or the game may crash. `CharacterUtility` contains some helper methods that already do this.
- Some IL2CPP classes are not converted correctly, e.g. `BalancedEnum`. You should still be able to access these classes via IL2CPP reflection.
- Calling IL2CPP methods can become very performance intensive in math heavy code. For this reason you should avoid Unity's math library as much as possible.
    - Use `System.Math` instead of `UnityEngine.Mathf`
    - Use `System.Numerics.Vector3` instead of `UnityEngine.Vector3`
    - There are extension methods to convert between the two `Vector3` classes: `v3.ToNET()` and `v3.ToIL2CPP()`
- [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki/Basic-usage) is very useful to work around broken IL2CPP stuff.

## Documentation

### Game Events
`GameEvents` is a helper class that let's you hook into common game events, e.g. `GameEvents.RunOnGameInit(callback)`.

### Scripting
`ModScripting` lets you register custom MiScript commands. To do this you must create a new IL2CPP class that implements your commands:
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
You can then use your methods ingame like this:
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
        _helper.Serialize(1, m_savedReference);
    }

    [HideFromIl2Cpp]
    protected override void deserializeMod(ref ModDeserializeHelper _helper)
    {
        base.deserializeMod(ref _helper);

        _helper.Deserialize(0, ref m_savedProperty);
        _helper.Deserialize(1, ref m_savedReference);
    }
}
```

When your plugin is loading you must register your component class with `ClassInjector.RegisterTypeInIl2Cpp<YourSaveableClass>()` and `ModSaveManager.RegisterType<YourSaveableClass>()`.

When you instantiate your component, it must be parented under a save root:
```csharp
var go = new GameObject();
go.transform.SetParent(SaveLoadSceneManager.transGetRoot());
go.AddComponent<YourSaveableClass>();
````

You must also load any assets that your component references on game init with `ModModularContainer.Load<T>("addressable_key")`, so that the savesystem knows about them. Unfortunately you also need to manually load any dependencies (e.g. materials) of these assets. If this becomes too much work, you could also consider working around this by keeping any models/particle effects/etc. outside of the saveroot and then manually create and destroy these objects when the saved component is created and destroyed.

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
