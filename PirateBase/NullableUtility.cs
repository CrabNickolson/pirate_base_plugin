using BepInEx;

namespace PirateBase;

public static class NullableUtility
{
    public static Il2CppSystem.Nullable<T> CreateNullable<T>(T _value, bool _forceHasNoValue = false) where T : Il2CppSystem.Object, new()
    {
        if (!_forceHasNoValue && _value != null)
            return new Il2CppSystem.Nullable<T>(_value);
        else
        {
            // hackfix empty constructor sometimes throwing exception
            var value = new Il2CppSystem.Nullable<T>(_value);
            value.hasValue = false;
            return value;
        }
    }

    public static Il2CppSystem.Nullable<T> ToIL2CPP<T>(this T? _value) where T : struct
    {
        if (_value.HasValue)
            return new Il2CppSystem.Nullable<T>(_value.Value);
        else
        {
            // hackfix empty constructor sometimes throwing exception
            var value = new Il2CppSystem.Nullable<T>(default(T));
            value.hasValue = false;
            return value;
        }
    }

    public static T? ToNET<T>(this Il2CppSystem.Nullable<T> _value) where T : struct
    {
        return _value != null && _value.HasValue ? _value.Value : null;
    }
}
