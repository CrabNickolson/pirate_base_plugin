using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace PirateBase;

public static class ShaderUtility
{
    private static Shader s_standardShaderHideVC;
    private static Shader s_standardShaderShowVC;

    public static Shader FindShader(string _name)
    {
        // for some reason, loading shaders with Shader.Find() doesn't work.
        // instead just search through exisiting objects to find the shader we want.
        var renderers = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader.name == _name)
                return renderer.sharedMaterial.shader;
        }
        return null;
    }

    public static Shader FindStandardHideVCShader()
    {
        if (s_standardShaderHideVC == null)
            s_standardShaderHideVC = FindShader("Mimimi/Standard/Standard (Metallic) HideVC");
        return s_standardShaderHideVC;
    }


    public static Shader FindStandardShowVCShader()
    {
        if (s_standardShaderShowVC == null)
            s_standardShaderShowVC = FindShader("Mimimi/Standard/Standard (Metallic) ShowVC");
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
}
