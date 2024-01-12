using BepInEx;
using BepInEx.Unity.IL2CPP;
using UnityEngine;
using Mimimi.Animation;

namespace PirateBase;

public static class CharacterUtility
{
    public static MiCharacterImpulseHandler.ImpulseOptionsDamage CreateDamageOptions(
        int _iDamage,
        MiCharacter _charOrigin,
        Skill.SkillType _eSkillTypeOrigin,
        float _fDurationWaitBeforeImpactKill = 0f,
        float? _fDurationVisible = null,
        float _fPointOfNoReturnTime = 0f,
        bool _bApplyHealthAnimationOnFinish = true,
        bool _bAutomaticOriginAndTargetYAdjustment = false,
        bool _bBadFortune = false,
        bool _bIgnoreGodMode = false,
        bool _bSkipAnimation = false,
        bool _bSkipLevelEvents = false,
        bool _bSkipVoice = false,
        bool _bTransferInventory = false,
        bool _bDoCharTypeSpecificReaction = true,
        bool _bKeepInGlobalMemory = false,
        Vector3? _v3InDirection = null,
        Vector3 _v3InRotationOffset = default,
        MiAnimAction _actionSlash = null,
        MiAnimAction _actionStruggle = null,
        MiAnimAction _actionFall = null,
        MiAnimAction _actionLie = null,
        MiSfxInfoObject _sfxOnKill = null,
        MiTransformable _transOrigin = null,
        MiAnimHierarchyNode _animHierarchyNode = null)
    {
        return MiCharacterImpulseHandler.ImpulseOptionsDamage.Create(
            _iDamage,
            _charOrigin,
            _eSkillTypeOrigin,
            _fDurationWaitBeforeImpactKill,
            _fDurationVisible.ToIL2CPP(),
            _fPointOfNoReturnTime,
            _bApplyHealthAnimationOnFinish,
            _bAutomaticOriginAndTargetYAdjustment,
            _bBadFortune,
            _bIgnoreGodMode,
            _bSkipAnimation,
            _bSkipLevelEvents,
            _bSkipVoice,
            _bTransferInventory,
            _bDoCharTypeSpecificReaction,
            _bKeepInGlobalMemory,
            _v3InDirection.ToIL2CPP(),
            _v3InRotationOffset,
            _actionSlash,
            _actionStruggle,
            _actionFall,
            _actionLie,
            _sfxOnKill,
            _transOrigin,
            _animHierarchyNode);
    }

    public static MiCharacterImpulseHandler.ImpulseOptionsKnockout CreateKnockoutOptions(
            MiTransformable _transOrigin,
            MiCharacter _charOrigin,
            Skill.SkillType _eSkillTypeOrigin,
            bool _bApplyHealthAnimationOnFinish = true,
            bool _bApplyTieUpIKDuringAnimation = true,
            bool _bApplyTieUpStateOnFinish = true,
            bool _bAutomaticOriginAndTargetYAdjustment = false,
            bool _bBadFortune = false,
            bool _bHideUI = false,
            bool _bTransferInventory = false,
            bool _bSkipAnimation = false,
            bool _bSkipLevelEvents = false,
            bool _bSkipNotifyController = false,
            bool _bDoCharTypeSpecificReaction = true,
            bool _bKeepInGlobalMemory = false,
            float? _fDurationVisible = null,
            float _fDurationWaitBeforeImpact = 0f,
            float _fPointOfNoReturnTime = 0f,
            MiAnimAction _actionSlash = null,
            MiAnimAction _actionStruggle = null,
            MiAnimAction _actionFall = null,
            MiAnimHierarchyNode _animHierarchyNode = null)
    {
        return MiCharacterImpulseHandler.ImpulseOptionsKnockout.Create(
            _transOrigin,
            _charOrigin,
            _eSkillTypeOrigin,
            _bApplyHealthAnimationOnFinish,
            _bApplyTieUpIKDuringAnimation,
            _bApplyTieUpStateOnFinish,
            _bAutomaticOriginAndTargetYAdjustment,
            _bBadFortune,
            _bHideUI,
            _bTransferInventory,
            _bSkipAnimation,
            _bSkipLevelEvents,
            _bSkipNotifyController,
            _bDoCharTypeSpecificReaction,
            _bKeepInGlobalMemory,
            _fDurationVisible.ToIL2CPP(),
            _fDurationWaitBeforeImpact,
            _fPointOfNoReturnTime,
            _actionSlash,
            _actionStruggle,
            _actionFall,
            _animHierarchyNode);
    }

    public static MiCharacterProcessOptionsRevive CreateReviveOptions(
        MiCharacter _charOrigin,
        MiCharacter _charTarget,
        Skill.SkillType _eSkillTypeOrigin,
        bool _bHideUI = false,
        bool _bReviveExterminated = false,
        bool _bReviveHidden = false,
        bool _bReviveCatatonic = false,
        bool _bSkipAnimation = false,
        bool _bPlayVO = true,
        bool _bPlaySfx = true,
        float? _fDuration = null,
        int _iRecoverHealth = -1,
        MiAnimAction _actionStruggle = null,
        MiAnimAction _actionRevived = null,
        MiAnimAction _actionOnCancel = null,
        BodyState _eDestinationBodyState = BodyState.stand,
        CharacterReviveVFXController _poolVFXController = null,
        bool _bRestoreShield = false)
    {
        return new MiCharacterProcessOptionsRevive(
            _charOrigin,
            _charTarget,
            _eSkillTypeOrigin,
            _bHideUI,
            _bReviveExterminated,
            _bReviveHidden,
            _bReviveCatatonic,
            _bSkipAnimation,
            _bPlayVO,
            _bPlaySfx,
            _fDuration.ToIL2CPP(),
            _iRecoverHealth,
            _actionStruggle,
            _actionRevived,
            _actionOnCancel,
            _eDestinationBodyState,
            _poolVFXController,
            _bRestoreShield);
    }

    public static MiCharacterProcessOptionsThrow CreateThrowOptions(
        MiCharacter _charOrigin,
        MiCharacter _charTarget,
        MiFlying _flying,
        bool _bCanKnockoutCharacter = true,
        bool _bReapplyTiedUpAnimation = true,
        bool _bQueueActions = false,
        bool _bSkipLandAnimation = false,
        bool _bSkipLandSfx = false,
        NoiseEmitter.NoiseEmitterSettings _noiseOnHit = null,
        MiAnimAction _actionFly = null,
        MiAnimAction _actionHitGround = null,
        MiAnimHierarchyNode _animHierarchyNodeTarget = null,
        IObjectCatcher _objectCatcher = null)
    {
        return new MiCharacterProcessOptionsThrow(
            _charOrigin,
            _charTarget,
            _flying,
            _bCanKnockoutCharacter,
            _bReapplyTiedUpAnimation,
            _bQueueActions,
            _bSkipLandAnimation,
            _bSkipLandSfx,
            NullableUtility.CreateNullable<NoiseEmitter.NoiseEmitterSettings>(CreateNoiseSettings(_charOrigin), _forceHasNoValue: true), // TODO can't set this properly because the game explodes
            _actionFly,
            _actionHitGround,
            _animHierarchyNodeTarget,
            _objectCatcher);
    }

    public static NoiseEmitter.NoiseEmitterSettings CreateNoiseSettings(MiCharacter _character)
    {
        // NoiseEmitterSettings constructor is broken, probably because of BalancedNoiseType
        return _character.movementNoise.m_noiseEmitter.settings.MemberwiseClone().Cast<NoiseEmitter.NoiseEmitterSettings>();
    }
}
