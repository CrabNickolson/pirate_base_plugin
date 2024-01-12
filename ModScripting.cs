using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using HarmonyLib;
using UnityEngine;
using Mimimi.MiScript;

using CollectionsNET = System.Collections.Generic;
using CollectionsIL2CPP = Il2CppSystem.Collections.Generic;
using MethodInfoNET = System.Reflection.MethodInfo;
using MethodInfoIL2CPP = Il2CppSystem.Reflection.MethodInfo;
using BindingFlagsNET = System.Reflection.BindingFlags;
using BindingFlagsIL2CPP = Il2CppSystem.Reflection.BindingFlags;
using System.Linq.Expressions;

namespace PirateBase;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class ModScriptMethodAttribute : System.Attribute
{
    private string m_name;
    private string m_helpText;

    public string name => m_name;
    public string helpText => m_helpText;

    public ModScriptMethodAttribute(string _name, string _helpText = null)
    {
        m_name = _name;
        m_helpText = _helpText;
    }
}

public static class ModScripting
{
    private static CollectionsNET.Dictionary<System.IntPtr, Il2CppSystem.Object> s_instanceDict = new();
    private static CollectionsIL2CPP.Dictionary<string, CollectionsIL2CPP.List<MethodInfoIL2CPP>> s_commandsDict = new();

    public static void Init()
    {
        Harmony.CreateAndPatchAll(typeof(ScriptingPatches));
    }

    public static void RegisterLibrary(Il2CppSystem.Object _instance)
    {
        if (_instance == null)
            return;

        var netType = _instance.GetType();
        var il2cppType = Il2CppType.From(_instance.GetType());
        if (il2cppType == null || s_instanceDict.ContainsKey(il2cppType.Pointer))
            return;

        s_instanceDict.Add(il2cppType.Pointer, _instance);

        // TODO can't have multiple methods with the same name, because miscript tries to access parameter name, which isn't set
        // TODO can't return ienumerators

        var netMethods = netType.GetMethods(BindingFlagsNET.Instance | BindingFlagsNET.Public | BindingFlagsNET.NonPublic);
        var il2cppMethods = il2cppType.GetMethods(BindingFlagsIL2CPP.Instance | BindingFlagsIL2CPP.Public | BindingFlagsIL2CPP.NonPublic);
        foreach (var netMethod in netMethods)
        {
            var attribute = System.Attribute.GetCustomAttribute(netMethod, typeof(ModScriptMethodAttribute)) as ModScriptMethodAttribute;
            if (attribute != null)
            {
                var il2cppMethod = findIl2CPPMethodInfo(netMethod, il2cppMethods);
                if (il2cppMethod != null)
                    registerCommand(attribute.name, il2cppMethod);
                else
                    Plugin.PluginLog.LogWarning($"Did not register miscript method {netMethod.Name} because no matching il2cpp method was found!");
            }
        }
    }

    private static MethodInfoIL2CPP findIl2CPPMethodInfo(MethodInfoNET _method, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<MethodInfoIL2CPP> _il2cppMethods)
    {
        var netParameters = _method.GetParameters();

        foreach (var il2cppMethod in _il2cppMethods)
        {
            if (il2cppMethod.Name == _method.Name)
            {
                var il2cppParameters = il2cppMethod.GetParameters();
                if (il2cppParameters.Length != netParameters.Length)
                    continue;

                bool allParametersValid = true;
                for (int i = 0; i < netParameters.Length; i++)
                {
                    var parameterType = Il2CppType.From(netParameters[i].ParameterType);
                    if (il2cppParameters[i].ParameterType != parameterType)
                    {
                        allParametersValid = false;
                        break;
                    }
                }

                if (allParametersValid)
                    return il2cppMethod;
            }
        }

        return null;
    }

    private static void registerCommand(string _name, MethodInfoIL2CPP _il2cppMethod)
    {
        if (!s_commandsDict.TryGetValue(_name, out var methodList))
        {
            methodList = new CollectionsIL2CPP.List<MethodInfoIL2CPP>();
            s_commandsDict.Add(_name, methodList);
        }

        methodList.Add(_il2cppMethod);
    }

    private static bool executeCommand(MiScript _script, string _strCommand, MiScript.MiScriptCallback _callback, IMiScriptCoroutineRunner _coroutineRunner)
    {
        if (_strCommand.StartsWith("#"))
        {
            return false;
        }

        var input = MiScriptInput.Parse(_strCommand);
        if (input.Parts.Count == 0 || !s_commandsDict.ContainsKey(input.Parts[0]))
        {
            return true;
        }

        MiScriptResult result = new MiScriptResult() { Command = _strCommand, Value = null };

        var originalCommands = _script.Database.m_dictCommands;
        _script.Database.m_dictCommands = s_commandsDict;

        ResolveMethodResult resolveMethodResult;
        MiScriptCommand command;
        MethodInfoIL2CPP methodIl2CPP;
        try
        {
            resolveMethodResult = _script.m_database.TryResolve(input, out command, out methodIl2CPP);
            if (!resolveMethodResult.IsSuccess)
            {
                result.Value = new Il2CppSystem.ArgumentException($"Error in Resolving Method: {resolveMethodResult}.");
                _callback?.Invoke(result);
                return false;
            }
        }
        catch (System.Exception e)
        {
            result.Value = new Il2CppSystem.ArgumentException($"Error in Resolving Method: {e.Message}.");
            _callback?.Invoke(result);
            return false;
        }
        finally
        {
            _script.Database.m_dictCommands = originalCommands;
        }

        Il2CppSystem.Object methodResult;
        try
        {
            var parametersOfType = _script.m_database.GetParametersOfType(command, methodIl2CPP, result);

            var instance = s_instanceDict[methodIl2CPP.DeclaringType.Pointer];
            methodResult = methodIl2CPP.Invoke(instance, parametersOfType);
        }
        catch (System.Exception e)
        {
            methodResult = new Il2CppSystem.Exception(e.Message); // TODO is this okay?
        }

        if (methodResult != null && (methodResult.TryCast<CollectionsIL2CPP.IEnumerable<MiScriptLine>>() != null || methodResult.TryCast<Il2CppSystem.Collections.IEnumerator>() != null))
        {
            result.Value = new Il2CppSystem.ArgumentException($"Modded commands cannot currently return IEnumerable.");
            _callback?.Invoke(result);
            return false;
        }

        if (command.PostExecutionOperation == PostExecutionOperation.AssignToTarget)
            _script.m_database.SaveResultToTarget(command.PostExecutionTarget, methodResult);

        bool isVoid = methodIl2CPP.ReturnType == Il2CppType.From(typeof(void));
        if (!isVoid)
            _script.m_database.SaveResultToTarget("_", methodResult);

        result.Value = isVoid ? new MiScriptResultValueVoid() : methodResult;
        _callback?.Invoke(result);

        Debug.Log($"(MiScript) {_strCommand}" + (!isVoid ? $" -> {methodResult}" : string.Empty));

        return false;
    }

    internal class ScriptingPatches
    {
        [HarmonyPatch(typeof(MiScript), nameof(MiScript.Execute), typeof(string), typeof(MiScript.MiScriptCallback), typeof(IMiScriptCoroutineRunner))]
        [HarmonyPrefix]
        public static bool patchExecute(MiScript __instance, string _strCommand, MiScript.MiScriptCallback _callback, IMiScriptCoroutineRunner _coroutineRunner)
        {
            Plugin.PluginLog.LogInfo("patchExecute");
            return ModScripting.executeCommand(__instance, _strCommand, _callback, _coroutineRunner);
        }
    }
}
