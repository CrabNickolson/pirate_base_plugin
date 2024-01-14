using BepInEx;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

using Vector2NET = System.Numerics.Vector2;
using Vector2IL2CPP = UnityEngine.Vector2;
using Vector3NET = System.Numerics.Vector3;
using Vector3IL2CPP = UnityEngine.Vector3;

namespace PirateBase;

public static class ClassExtensions
{
    public static Vector2IL2CPP ToIL2CPP(this Vector2NET _v)
    {
        return new Vector2IL2CPP(_v.X, _v.Y);
    }

    public static Vector3IL2CPP ToIL2CPP(this Vector3NET _v)
    {
        return new Vector3IL2CPP(_v.X, _v.Y, _v.Z);
    }

    public static Vector2NET ToNET(this Vector2IL2CPP _v)
    {
        return new Vector2NET(_v.x, _v.y);
    }

    public static Vector3NET ToNET(this Vector3IL2CPP _v)
    {
        return new Vector3NET(_v.x, _v.y, _v.z);
    }

    public static Vector3NET[] ToNET(this Il2CppStructArray<Vector3IL2CPP> _array)
    {
        Vector3NET[] result = new Vector3NET[_array.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = _array[i].ToNET();
        return result;
    }

    public static Il2CppStructArray<Vector2IL2CPP> ToIL2CPP(this Vector2NET[] _array)
    {
        var result = new Il2CppStructArray<Vector2IL2CPP>(_array.Length);
        for (int i = 0; i < _array.Length; i++)
            result[i] = _array[i].ToIL2CPP();
        return result;
    }

    public static Il2CppStructArray<Vector3IL2CPP> ToIL2CPP(this Vector3NET[] _array)
    {
        var result = new Il2CppStructArray<Vector3IL2CPP>(_array.Length);
        for (int i = 0; i < _array.Length; i++)
            result[i] = _array[i].ToIL2CPP();
        return result;
    }

    public static Il2CppSystem.Collections.Generic.List<T> ToIL2CPP<T>(this System.Collections.Generic.List<T> _list) where T : Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase
    {
        // Adding elements to an Il2Cpp list corrupts memory...
        int listCapacity = _list.Count > 0 ? Mathf.Max(Mathf.NextPowerOfTwo(_list.Count), 4) : 0;
        var il2cppArray = new Il2CppReferenceArray<T>(listCapacity);
        for (int i = 0; i < _list.Count; i++)
            il2cppArray[i] = _list[i];

        var list = new Il2CppSystem.Collections.Generic.List<T>();
        list._items = il2cppArray;
        list._size = _list.Count;

        return list;
    }

    public unsafe static void CopyToIL2CPP(this Vector3NET[] _array, Il2CppStructArray<Vector3IL2CPP> _il2cppArray)
    {
        // Creating many Il2Cpp structs causes (GC?) freezes, so we need to avoid that as much as possible.
        int count = System.Math.Min(_array.Length, _il2cppArray.Length);
        System.IntPtr ptr = _il2cppArray.Pointer;
        Vector3IL2CPP v3 = new Vector3IL2CPP();
        for (int i = 0; i < count; i++)
        {
            v3.Set(_array[i].X, _array[i].Y, _array[i].Z);
            *(Vector3IL2CPP*)((byte*)System.IntPtr.Add(ptr, 4 * System.IntPtr.Size).ToPointer() + (nint)i * (nint)sizeof(Vector3IL2CPP)) = v3;
        }
    }

    public unsafe static void CopyToIL2CPP(this Vector2NET[] _array, Il2CppStructArray<Vector2IL2CPP> _il2cppArray)
    {
        int count = System.Math.Min(_array.Length, _il2cppArray.Length);
        System.IntPtr ptr = _il2cppArray.Pointer;
        Vector2IL2CPP v2 = new Vector2IL2CPP();
        for (int i = 0; i < count; i++)
        {
            v2.Set(_array[i].X, _array[i].Y);
            *(Vector2IL2CPP*)((byte*)System.IntPtr.Add(ptr, 4 * System.IntPtr.Size).ToPointer() + (nint)i * (nint)sizeof(Vector2IL2CPP)) = v2;
        }
    }

    public unsafe static void CopyColorToIL2CPP(this Vector3NET[] _array, Il2CppStructArray<Color> _il2cppArray, float _alpha)
    {
        int count = System.Math.Min(_array.Length, _il2cppArray.Length);
        System.IntPtr ptr = _il2cppArray.Pointer;
        Color c = new Color(0, 0, 0, _alpha);
        for (int i = 0; i < count; i++)
        {
            c.r = _array[i].X;
            c.g = _array[i].Y;
            c.b = _array[i].Z;
            *(Color*)((byte*)System.IntPtr.Add(ptr, 4 * System.IntPtr.Size).ToPointer() + (nint)i * (nint)sizeof(Color)) = c;
        }
    }

    public static void SetNoiseType(this NoiseEmitter.NoiseEmitterSettings _noiseSettings, NoiseDetection.NoiseType _type)
    {
        // BalancedNoiseType is broken, so we can only touch it via reflection
        var type1 = Il2CppInterop.Runtime.Il2CppType.Of<NoiseEmitter.NoiseEmitterSettings>();
        var field1 = type1.GetField("noiseType");
        var noiseType = field1.GetValue(_noiseSettings);
        var type2 = Il2CppSystem.Type.GetType("BalancedNoiseType");
        var field2 = type2.GetField("m_tValue", Il2CppSystem.Reflection.BindingFlags.NonPublic | Il2CppSystem.Reflection.BindingFlags.Instance);
        field2.SetValue(noiseType, (int)_type);
        field1.SetValue(_noiseSettings, noiseType);
    }

    public static Mesh CreateReadableCopy(this Mesh _mesh, bool _validate = false)
    {
        // https://forum.unity.com/threads/reading-meshes-at-runtime-that-are-not-enabled-for-read-write.950170/#post-8891865

        Mesh meshCopy = new Mesh();
        meshCopy.indexFormat = _mesh.indexFormat;

        // Handle vertices.
        GraphicsBuffer verticesBuffer = _mesh.GetVertexBuffer(0);
        int totalSize = verticesBuffer.stride * verticesBuffer.count;
        var data = new Il2CppStructArray<byte>(totalSize);
        verticesBuffer.InternalGetData(data.Cast<Il2CppSystem.Array>(), 0, 0, data.Length, System.Runtime.InteropServices.Marshal.SizeOf<byte>());
        meshCopy.SetVertexBufferParams(_mesh.vertexCount, _mesh.GetVertexAttributes());
        meshCopy.InternalSetVertexBufferDataFromArray(0, data.Cast<Il2CppSystem.Array>(), 0, 0, totalSize, System.Runtime.InteropServices.Marshal.SizeOf<byte>(), UnityEngine.Rendering.MeshUpdateFlags.Default);
        verticesBuffer.Release();

        if (_validate)
        {
            // Reading meshes back from GPU memory does not work arbitrarily for some meshes.
            // We can detect this by checking if our result buffer is all zeros.
            bool vertexDataValid = false;
            for (int i = 0; i < totalSize; i++)
            {
                if (data[i] != 0)
                {
                    vertexDataValid = true;
                    break;
                }
            }
            if (!vertexDataValid)
            {
                Object.Destroy(meshCopy);
                return null;
            }
        }

        // Handle triangles.
        meshCopy.subMeshCount = _mesh.subMeshCount;
        GraphicsBuffer indexesBuffer = _mesh.GetIndexBuffer();
        int tot = indexesBuffer.stride * indexesBuffer.count;
        var indexesData = new Il2CppStructArray<byte>(tot);
        indexesBuffer.InternalGetData(indexesData.Cast<Il2CppSystem.Array>(), 0, 0, indexesData.Length, System.Runtime.InteropServices.Marshal.SizeOf<byte>());
        meshCopy.SetIndexBufferParams(indexesBuffer.count, _mesh.indexFormat);
        meshCopy.InternalSetIndexBufferDataFromArray(indexesData.Cast<Il2CppSystem.Array>(), 0, 0, tot, System.Runtime.InteropServices.Marshal.SizeOf<byte>(), UnityEngine.Rendering.MeshUpdateFlags.Default);
        indexesBuffer.Release();

        // Restore submesh structure.
        uint currentIndexOffset = 0;
        for (int i = 0; i < meshCopy.subMeshCount; i++)
        {
            // Normal property setters on SubMeshDescriptor are broken for some reason.
            uint subMeshIndexCount = _mesh.GetIndexCount(i);
            var desc = new UnityEngine.Rendering.SubMeshDescriptor();
            desc._indexStart_k__BackingField = (int)currentIndexOffset;
            desc._indexCount_k__BackingField = (int)subMeshIndexCount;
            desc._topology_k__BackingField = MeshTopology.Triangles;
            desc._bounds_k__BackingField = default;
            desc._baseVertex_k__BackingField = 0;
            desc._firstVertex_k__BackingField = 0;
            desc._vertexCount_k__BackingField = 0;

            meshCopy.SetSubMesh(i, desc);
            currentIndexOffset += subMeshIndexCount;
        }

        // Recalculate normals and bounds.
        meshCopy.RecalculateNormals();
        meshCopy.RecalculateBounds();

        return meshCopy;
    }
}
