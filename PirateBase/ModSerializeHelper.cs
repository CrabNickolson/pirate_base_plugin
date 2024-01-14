using BepInEx;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem;

namespace PirateBase;

public struct ModSerializeHelper
{
    private Dictionary<long, Object> m_dictFields;
    public Dictionary<long, Object> dictFields => m_dictFields;

    public ModSerializeHelper(Dictionary<long, Object> _dictFields)
    {
        m_dictFields = _dictFields;
    }

    public void Serialize(long _id, bool _value)
    {
        m_dictFields.Add(_id, _value);
    }

    public void Serialize(long _id, int _value)
    {
        m_dictFields.Add(_id, _value);
    }

    public void Serialize(long _id, uint _value)
    {
        m_dictFields.Add(_id, _value);
    }

    public void Serialize(long _id, float _value)
    {
        m_dictFields.Add(_id, _value);
    }

    public void Serialize(long _id, UnityEngine.Vector3 _value)
    {
        m_dictFields.Add(_id, _value.BoxIl2CppObject());
    }

    public void Serialize(long _id, string _value)
    {
        if (_value != null)
            m_dictFields.Add(_id, _value);
    }

    public void Serialize(long _id, UnityEngine.ScriptableObject _value)
    {
        if (_value != null)
            m_dictFields.Add(_id, SaveLoadIDManager.iRefToID_ScriptableObj(_value));
    }

    public void Serialize(long _id, UnityEngine.Material _value)
    {
        if (_value != null)
            m_dictFields.Add(_id, SaveLoadIDManager.iRefToID_Material(_value));
    }

    public void SerializeUnityObject(long _id, UnityEngine.Object _value)
    {
        if (_value != null)
            m_dictFields.Add(_id, SaveLoadIDManager.iRefToID_UnityObj(_value));
    }

    public void SerializeSystemObject(long _id, Object _value)
    {
        if (_value != null)
            m_dictFields.Add(_id, _value);
    }
}

public struct ModDeserializeHelper
{
    private Dictionary<long, Object> m_dictFields;
    private Dictionary<long, UnityEngine.Object> m_dictObjects;
    private Dictionary<long, UnityEngine.Object> m_dictAssets;
    private Dictionary<long, Object> m_dictClasses;

    public Dictionary<long, Object> dictFields => m_dictFields;
    public Dictionary<long, UnityEngine.Object> dictObjects => m_dictObjects;
    public Dictionary<long, UnityEngine.Object> dictAssets => m_dictAssets;
    public Dictionary<long, Object> dictClasses => m_dictClasses;

    public ModDeserializeHelper(Dictionary<long, Object> _dictFields, Dictionary<long, UnityEngine.Object> _dictObjects,
        Dictionary<long, UnityEngine.Object> _dictAssets, Dictionary<long, Object> _dictClasses)
    {
        m_dictFields = _dictFields;
        m_dictObjects = _dictObjects;
        m_dictAssets = _dictAssets;
        m_dictClasses = _dictClasses;
    }

    public bool Deserialize(long _id, ref bool _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = value.Unbox<bool>();
            return true;
        }
        return false;
    }

    public bool Deserialize(long _id, ref int _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = value.Unbox<int>();
            return true;
        }
        return false;
    }

    public bool Deserialize(long _id, ref uint _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = value.Unbox<uint>();
            return true;
        }
        return false;
    }

    public bool Deserialize(long _id, ref float _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = value.Unbox<float>();
            return true;
        }
        return false;
    }

    public bool Deserialize(long _id, ref UnityEngine.Vector3 _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = value.Unbox<UnityEngine.Vector3>();
            return true;
        }
        return false;
    }

    public bool Deserialize(long _id, ref System.Numerics.Vector3 _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            UnityEngine.Vector3 il2cppValue = value.Unbox<UnityEngine.Vector3>();
            _value = il2cppValue.ToNET();
            return true;
        }
        return false;
    }

    public bool Deserialize(long _id, ref string _value)
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = value.Cast<String>();
            return true;
        }
        return false;
    }

    public bool DeserializeScriptableObject<T>(long _id, ref T _value) where T : UnityEngine.ScriptableObject
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = SaveLoadTypeDeserializer.recreateRefsFromIDs(m_dictObjects, m_dictAssets, m_dictClasses, value).Cast<T>();
            return true;
        }
        return false;
    }

    public bool DeserializeMaterial<T>(long _id, ref T _value) where T : UnityEngine.Material
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = SaveLoadTypeDeserializer.recreateRefsFromIDs(m_dictObjects, m_dictAssets, m_dictClasses, value).Cast<T>();
            return true;
        }
        return false;
    }

    public bool DeserializeUnityObject<T>(long _id, ref T _value) where T : UnityEngine.Object
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = SaveLoadTypeDeserializer.recreateRefsFromIDs_UnityObj(m_dictObjects, m_dictAssets, value).Cast<T>();
            return true;
        }
        return false;
    }

    public bool DeserializeSystemObject<T>(long _id, ref T _value) where T : Object
    {
        if (m_dictFields.TryGetValue(_id, out var value))
        {
            _value = SaveLoadTypeDeserializer.recreateRefsFromIDs_SystemObj(m_dictClasses, value).Cast<T>();
            return true;
        }
        return false;
    }
}
