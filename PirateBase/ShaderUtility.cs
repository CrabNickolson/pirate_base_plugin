using BepInEx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PirateBase;

public static class ShaderUtility
{
    private static Shader s_standardShaderHideVC;
    private static Shader s_standardShaderShowVC;

    //

    public static Shader FindStandardHideVCShader()
    {
        if (s_standardShaderHideVC == null)
            s_standardShaderHideVC = loadShader("Assets/Shader/Mimimi/include/Standard/MiStandardMetallic-HideVC.shader");
        return s_standardShaderHideVC;
    }


    public static Shader FindStandardShowVCShader()
    {
        if (s_standardShaderShowVC == null)
            s_standardShaderShowVC = loadShader("Assets/Shader/Mimimi/include/Standard/MiStandardMetallic-ShowVC.shader");
        return s_standardShaderShowVC;
    }

    public static void ReplaceObjectShaders(GameObject _go, Shader _shader)
    {
        var renderers = _go.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials;
            foreach (var mat in materials)
            {
                if (mat != null)
                {
                    mat.shader = _shader;
                }
            }
        }
    }

    private static Shader loadShader(string _key)
    {
        var op = Addressables.LoadAssetAsync<Shader>(_key);
        op.WaitForCompletion();
        if (op.Status == AsyncOperationStatus.Succeeded)
            return op.Result;
        else
            return null;
    }
}
