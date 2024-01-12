using BepInEx;
using Il2CppSystem.Collections.Generic;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.Attributes;

namespace PirateBase;

public class ModSaveable : MiSaveable
{
    private UnityEngine.Transform m_cachedTransform;
    public new UnityEngine.Transform trans
    {
        get
        {
            if (m_cachedTransform == null)
                m_cachedTransform = this.transform;
            return m_cachedTransform;
        }
    }

    public ModSaveable(System.IntPtr ptr) : base(ptr) { }

    public ModSaveable() : base(ClassInjector.DerivedConstructorPointer<ModSaveable>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public override void AwakeDelayed()
    {
    }

    public override void MiAwake()
    {
        m_eAwakeState = AwakeState.AwakeCalled;
    }

    public override void MiOnEnable()
    {
        m_bMiOnEnableCalledOnce = true;
        m_bMiEnabled = true;
    }

    public override void OnEnableDelayed()
    {

    }

    public override void MiOnDisable()
    {
        m_bMiEnabled = false;
    }

    public override void MiStart()
    {
        m_eAwakeState = AwakeState.StartCalled;
    }

    public override void StartDelayed()
    {

    }

    public override void OnDestroy()
    {
        if (wantsMiOnDestroyCall)
            MiOnDestroy();
    }

    public override void OnSerialize()
    {
        ModSaveManager.RegisterObjectToSave(this);
    }

    public virtual void OnSerializeMod()
    {
        RefID id = SaveLoadIDManager.dIDs[this];
        var modDataStructure = ModSaveManager.GetDataStructure(id.m_lID);
        var dict = modDataStructure.m_dIDObj;

        var helper = new ModSerializeHelper(dict);
        serializeMod(ref helper);
    }

    [HideFromIl2Cpp]
    protected virtual void serializeMod(ref ModSerializeHelper _helper)
    {

    }

    public override void OnLoadingFinished()
    {
        RefID id = SaveLoadIDManager.dIDs[this];
        var modDataStructure = ModSaveManager.GetDataStructure(id.m_lID);
        var dict = modDataStructure.m_dIDObj;

        var helper = new ModDeserializeHelper(dict, ModSaveManager.GetObjectDict(), ModSaveManager.GetAssetDict(), ModSaveManager.GetClassDict());
        deserializeMod(ref helper);
    }

    [HideFromIl2Cpp]
    protected virtual void deserializeMod(ref ModDeserializeHelper _helper)
    {
    }
}
