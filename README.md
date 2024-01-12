# PirateBase

A [BepInEx](https://github.com/BepInEx/BepInEx) plugin for Shadow Gambit that wraps core game functionality and makes it easier to access for other plugins.

## IL2CPP
Shadow Gambit uses IL2CPP, which unfortunately makes modding fairly complicated. Here are some things to watch out for:
- Your cannot cast Il2CPP normally. Instead you must use the `Cast<T>` method.
- If you directly override a IL2CPP method in a subclass you cannot call `base.method()`, as that would cause an infinite recursion.
- Some IL2CPP structs have been converted into classes. Be careful not to initialize these with `default`.
- Some IL2CPP classes have not been converted correctly, e.g. `BalancedEnum`. You should still be able to access these classes via IL2CPP reflection.
- [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki/Basic-usage) is very useful to work around broken IL2CPP stuff.

## Events
`GameEvents` is a helper class that let's you hook into common game events, e.g. `GameEvents.RunOnGameInit(callback)`.

## Scripting
`ModScripting` lets you register custom miscript methods. To do this you must create a new class that implements your script methods:
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
}
```

You must register an instance of this class on game init with `ModScripting.RegisterLibrary(new YourScriptingClass());`

## Saving
To create a custom component that can be saved, create a new class that inherits from `ModSaveable`:

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

When your plugin is loaded you must register your component class with `ClassInjector.RegisterTypeInIl2Cpp<YourSaveableClass>()` and `ModSaveManager.RegisterType<YourSaveableClass>()`.

Your component must be parented under a save root:
```csharp
var go = new GameObject();
go.transform.SetParent(SaveLoadSceneManager.transGetRoot());
go.AddComponent<YourSaveableClass>();
````

You must also load any assets that your component references on game init with `ModModularContainer.Load<T>("addressable_key")`. Unfortunately you also need to register any dependencies (e.g. materials) of these assets manually. If this becomes too much work, you could also consider working around this by not saving any asset references. Instead you can keep any models/particle effects/etc. outside of the saveroot and then manually create and destroy these objects when the saved component is created and destroyed.
