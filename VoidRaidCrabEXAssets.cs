using EntityStates;
using MysticsRisky2Utils;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Audio;
using RoR2.Projectile;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode
{
    public class VoidRaidCrabEXAssets : BaseLoadableAsset
    {
		public static GameObject arenaEffectsPrefab;

        public static DamageAPI.ModdedDamageType disablePrimaryDamageType;
        public static SkillDef skillDisarmed;

		public static SkillFamily skillFamilyPrimary;
		public static SkillDef skillDisarmingOrbMinigun;
		public static SkillDef skillAheadLaserWithPool;
		public static SkillDef skillDevastatorBombMortar;

		public static SkillFamily skillFamilySecondary;
		public static SkillDef skillDirectLasers;
		public static SkillDef skillReaverBombPattern;
		
		public static SkillFamily skillFamilyUtility;
		public static SkillDef skillKnockUpEveryone;
		public static SkillDef skillVacuum;
		public static SkillDef skillReaverBombSnake;

		public static SkillFamily skillFamilySpecial;
		public static SkillDef skillScream;
		public static SkillDef skillScreamCallForHelp;
		public static SkillDef skillBeamSweep;

		public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            disablePrimaryDamageType = DamageAPI.ReserveDamageType();
			NetworkingAPI.RegisterMessageType<FireDevastatorMortar.RequestServerSpawnWarningEffect>();
		}

        public override void Load()
        {
			arenaEffectsPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/VoidRaidCrabEXArenaEffects.prefab");

            skillDisarmed = ScriptableObject.CreateInstance<SkillDef>();
            skillDisarmed.skillName = "ChallengeMode_Disarmed";
            skillDisarmed.skillNameToken = "CHALLENGEMODE_SKILL_DISARMED_NAME";
            skillDisarmed.skillDescriptionToken = "CHALLENGEMODE_SKILL_DISARMED_DESCRIPTION";
            skillDisarmed.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiCancelTargetingDummy.asset").WaitForCompletion().icon;
            skillDisarmed.activationStateMachineName = "Weapon";
            skillDisarmed.activationState = new SerializableEntityStateType(typeof(EntityStates.Idle));
            skillDisarmed.interruptPriority = InterruptPriority.Any;
            skillDisarmed.baseRechargeInterval = 0f;
            skillDisarmed.baseMaxStock = 0;
            skillDisarmed.rechargeStock = 0;
            skillDisarmed.requiredStock = 0;
            skillDisarmed.stockToConsume = 0;
            skillDisarmed.resetCooldownTimerOnUse = false;
            skillDisarmed.fullRestockOnAssign = false;
            skillDisarmed.dontAllowPastMaxStocks = false;
            skillDisarmed.beginSkillCooldownOnSkillEnd = false;
            skillDisarmed.cancelSprintingOnActivation = false;
            skillDisarmed.forceSprintDuringState = false;
            skillDisarmed.canceledFromSprinting = false;
            skillDisarmed.isCombatSkill = false;
            skillDisarmed.mustKeyPress = true;
            ChallengeModeContent.Resources.skillDefs.Add(skillDisarmed);

			// enemy skills
			skillDisarmingOrbMinigun = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillDisarmingOrbMinigun).name = "ChallengeMode_VoidRaidCrabEXBodyDisarmingOrbMinigun";
			skillDisarmingOrbMinigun.skillName = "DisarmingOrbMinigun";
			skillDisarmingOrbMinigun.activationStateMachineName = "Weapon";
			skillDisarmingOrbMinigun.activationState = new SerializableEntityStateType(typeof(FireDisarmingOrb));
			skillDisarmingOrbMinigun.interruptPriority = InterruptPriority.Skill;
			skillDisarmingOrbMinigun.baseRechargeInterval = 5f;
			skillDisarmingOrbMinigun.baseMaxStock = 12;
			skillDisarmingOrbMinigun.rechargeStock = 12;
			skillDisarmingOrbMinigun.requiredStock = 1;
			skillDisarmingOrbMinigun.stockToConsume = 1;
			skillDisarmingOrbMinigun.resetCooldownTimerOnUse = true;
			skillDisarmingOrbMinigun.fullRestockOnAssign = true;
			skillDisarmingOrbMinigun.dontAllowPastMaxStocks = true;
			skillDisarmingOrbMinigun.beginSkillCooldownOnSkillEnd = false;
			skillDisarmingOrbMinigun.cancelSprintingOnActivation = true;
			skillDisarmingOrbMinigun.forceSprintDuringState = false;
			skillDisarmingOrbMinigun.canceledFromSprinting = false;
			skillDisarmingOrbMinigun.isCombatSkill = true;
			skillDisarmingOrbMinigun.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillDisarmingOrbMinigun);
			FireDisarmingOrb.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMuzzleflashEyeMissiles.prefab").WaitForCompletion();
			FireDisarmingOrb.projectilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMissileProjectile.prefab").WaitForCompletion(), "ChallengeMode_VoidRaidCrabEXDisarmingOrb", true);
			// FireDisarmingOrb.projectilePrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(disablePrimaryDamageType);
			FireDisarmingOrb.projectilePrefab.AddComponent<ChallengeModeDisarmingProjectile>();
			FireDisarmingOrb.projectilePrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed = 120f;
			FireDisarmingOrb.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>().lookRange = 100f;
			FireDisarmingOrb.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 30f;

			skillAheadLaserWithPool = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillAheadLaserWithPool).name = "ChallengeMode_VoidRaidCrabEXBodyAheadLaserWithPool";
			skillAheadLaserWithPool.skillName = "AheadLaserWithPool";
			skillAheadLaserWithPool.activationStateMachineName = "Weapon";
			skillAheadLaserWithPool.activationState = new SerializableEntityStateType(typeof(ChargeAheadLaser));
			skillAheadLaserWithPool.interruptPriority = InterruptPriority.Skill;
			skillAheadLaserWithPool.baseRechargeInterval = 5f;
			skillAheadLaserWithPool.baseMaxStock = 3;
			skillAheadLaserWithPool.rechargeStock = 3;
			skillAheadLaserWithPool.requiredStock = 1;
			skillAheadLaserWithPool.stockToConsume = 1;
			skillAheadLaserWithPool.resetCooldownTimerOnUse = true;
			skillAheadLaserWithPool.fullRestockOnAssign = true;
			skillAheadLaserWithPool.dontAllowPastMaxStocks = true;
			skillAheadLaserWithPool.beginSkillCooldownOnSkillEnd = false;
			skillAheadLaserWithPool.cancelSprintingOnActivation = true;
			skillAheadLaserWithPool.forceSprintDuringState = false;
			skillAheadLaserWithPool.canceledFromSprinting = false;
			skillAheadLaserWithPool.isCombatSkill = true;
			skillAheadLaserWithPool.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillAheadLaserWithPool);
			ChargeAheadLaser.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamChargeUp.prefab").WaitForCompletion();
			ChargeAheadLaser.laserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/LaserGolemGold.prefab").WaitForCompletion();
			FireAheadLaser._muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamMuzzleflash.prefab").WaitForCompletion();
			FireAheadLaser._tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeam.prefab").WaitForCompletion();
			FireAheadLaser._explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();
			FireAheadLaser._projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMultiBeamDotZone.prefab").WaitForCompletion();

			skillFamilyPrimary = ScriptableObject.CreateInstance<SkillFamily>();
            ((ScriptableObject)skillFamilyPrimary).name = "ChallengeMode_VoidRaidCrabEXBodyPrimaryFamily";
			skillFamilyPrimary.variants = new SkillFamily.Variant[]
            {
                new SkillFamily.Variant
                {
                    skillDef = skillDisarmingOrbMinigun
                },
				new SkillFamily.Variant
				{
					skillDef = skillAheadLaserWithPool
				}
			};
			skillFamilyPrimary.defaultVariantIndex = 0;
            ChallengeModeContent.Resources.skillFamilies.Add(skillFamilyPrimary);

			skillDirectLasers = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillDirectLasers).name = "ChallengeMode_VoidRaidCrabEXBodyDirectLasers";
			skillDirectLasers.skillName = "DirectLasers";
			skillDirectLasers.activationStateMachineName = "Weapon";
			skillDirectLasers.activationState = new SerializableEntityStateType(typeof(ChargeDirectLasers));
			skillDirectLasers.interruptPriority = InterruptPriority.Skill;
			skillDirectLasers.baseRechargeInterval = 15f;
			skillDirectLasers.baseMaxStock = 1;
			skillDirectLasers.rechargeStock = 1;
			skillDirectLasers.requiredStock = 1;
			skillDirectLasers.stockToConsume = 1;
			skillDirectLasers.resetCooldownTimerOnUse = true;
			skillDirectLasers.fullRestockOnAssign = true;
			skillDirectLasers.dontAllowPastMaxStocks = true;
			skillDirectLasers.beginSkillCooldownOnSkillEnd = false;
			skillDirectLasers.cancelSprintingOnActivation = true;
			skillDirectLasers.forceSprintDuringState = false;
			skillDirectLasers.canceledFromSprinting = false;
			skillDirectLasers.isCombatSkill = true;
			skillDirectLasers.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillDirectLasers);
			ChargeDirectLasers.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamChargeUp.prefab").WaitForCompletion();
			ChargeDirectLasers.laserPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/LaserGolemGold.prefab").WaitForCompletion();
			FireDirectLasers._muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamMuzzleflash.prefab").WaitForCompletion();
			FireDirectLasers._tracerEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/TracerVoidRaidCrabTripleBeam.prefab").WaitForCompletion();
			FireDirectLasers._explosionEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabTripleBeamExplosion.prefab").WaitForCompletion();

			skillReaverBombPattern = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillReaverBombPattern).name = "ChallengeMode_VoidRaidCrabEXBodyReaverBombPattern";
			skillReaverBombPattern.skillName = "ReaverBombPattern";
			skillReaverBombPattern.activationStateMachineName = "Weapon";
			skillReaverBombPattern.activationState = new SerializableEntityStateType(typeof(FireReaverBombPattern));
			skillReaverBombPattern.interruptPriority = InterruptPriority.Skill;
			skillReaverBombPattern.baseRechargeInterval = 15f;
			skillReaverBombPattern.baseMaxStock = 3;
			skillReaverBombPattern.rechargeStock = 3;
			skillReaverBombPattern.requiredStock = 1;
			skillReaverBombPattern.stockToConsume = 1;
			skillReaverBombPattern.resetCooldownTimerOnUse = true;
			skillReaverBombPattern.fullRestockOnAssign = true;
			skillReaverBombPattern.dontAllowPastMaxStocks = true;
			skillReaverBombPattern.beginSkillCooldownOnSkillEnd = false;
			skillReaverBombPattern.cancelSprintingOnActivation = true;
			skillReaverBombPattern.forceSprintDuringState = false;
			skillReaverBombPattern.canceledFromSprinting = false;
			skillReaverBombPattern.isCombatSkill = true;
			skillReaverBombPattern.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillReaverBombPattern);
			FireReaverBombPattern.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabFinalStandMuzzleflash.prefab").WaitForCompletion();
			FireReaverBombPattern.projectilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion(), "ChallengeMode_VoidRaidCrabEXGigaReaverBomb", true);
			var gigaBombScale = 20f;
			FireReaverBombPattern.projectilePrefab.GetComponent<ProjectileExplosion>().blastRadius = gigaBombScale;
			FireReaverBombPattern.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab = PrefabAPI.InstantiateClone(FireReaverBombPattern.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab, "ChallengeMode_VoidRaidCrabEXGigaReaverBombGhost", false);
			FireReaverBombPattern.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.Find("Scale").localScale = Vector3.one * gigaBombScale;

			skillDevastatorBombMortar = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillDevastatorBombMortar).name = "ChallengeMode_VoidRaidCrabEXBodyDevastatorBombMortar";
			skillDevastatorBombMortar.skillName = "DevastatorBombMortar";
			skillDevastatorBombMortar.activationStateMachineName = "Weapon";
			skillDevastatorBombMortar.activationState = new SerializableEntityStateType(typeof(FireDevastatorMortar));
			skillDevastatorBombMortar.interruptPriority = InterruptPriority.Skill;
			skillDevastatorBombMortar.baseRechargeInterval = 15f;
			skillDevastatorBombMortar.baseMaxStock = 3;
			skillDevastatorBombMortar.rechargeStock = 3;
			skillDevastatorBombMortar.requiredStock = 1;
			skillDevastatorBombMortar.stockToConsume = 1;
			skillDevastatorBombMortar.resetCooldownTimerOnUse = true;
			skillDevastatorBombMortar.fullRestockOnAssign = true;
			skillDevastatorBombMortar.dontAllowPastMaxStocks = true;
			skillDevastatorBombMortar.beginSkillCooldownOnSkillEnd = false;
			skillDevastatorBombMortar.cancelSprintingOnActivation = true;
			skillDevastatorBombMortar.forceSprintDuringState = false;
			skillDevastatorBombMortar.canceledFromSprinting = false;
			skillDevastatorBombMortar.isCombatSkill = true;
			skillDevastatorBombMortar.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillDevastatorBombMortar);
			FireDevastatorMortar.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMuzzleflashEyeMissiles.prefab").WaitForCompletion();
			FireDevastatorMortar.projectilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombletsProjectile.prefab").WaitForCompletion(), "ChallengeMode_VoidRaidCrabEXDevastatorMortar", true);
			FireDevastatorMortar.projectilePrefab.GetComponent<ProjectileImpactExplosion>().lifetime = 8f;
			FireDevastatorMortar.warningEffectPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/MortarWarning.prefab");
			FireDevastatorMortar.warningEffectPrefab.AddComponent<EffectComponent>();
			var vfxAttributes = FireDevastatorMortar.warningEffectPrefab.AddComponent<VFXAttributes>();
			vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
			vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
			FireDevastatorMortar.warningEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2.5f;
			var decalComponent = FireDevastatorMortar.warningEffectPrefab.transform.Find("Decal").gameObject.AddComponent<Decal>();
			decalComponent.transform.localScale = Vector3.one * 10f;
			var decalMaterial = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/matVoidRaidCrabEXWarningDecal.mat");
			decalMaterial.shader = Addressables.LoadAssetAsync<Shader>("Decalicious/DecaliciousDeferredDecal.shader").WaitForCompletion();
			decalMaterial.SetTexture("_MainTex", ChallengeModePlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/texMortarWarningCircle.png"));
			decalMaterial.SetTexture("_MaskTex", ChallengeModePlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/texMortarWarningCircleMask.png"));
			decalMaterial.SetColor("_Color", new Color(0.774869f, 0.14659683f, 5.3403134f, 1f));
			decalMaterial.SetFloat("_DecalBlendMode", 1f);
			decalMaterial.SetFloat("_AngleLimit", 0.6f);
			decalMaterial.SetFloat("_DecalLayer", 1f);
			decalComponent.Material = decalMaterial;
			decalComponent.Fade = 1f;
			var objectScaleCurve = FireDevastatorMortar.warningEffectPrefab.AddComponent<ObjectScaleCurve>();
			objectScaleCurve.overallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			objectScaleCurve.useOverallCurveOnly = true;
			objectScaleCurve.timeMax = 0.33f;
			FireDevastatorMortar.warningEffectPrefab.AddComponent<RotateObject>().rotationSpeed = new Vector3(0f, 3f, 0f);
			ChallengeModeContent.Resources.effectPrefabs.Add(FireDevastatorMortar.warningEffectPrefab);

			skillFamilySecondary = ScriptableObject.CreateInstance<SkillFamily>();
			((ScriptableObject)skillFamilySecondary).name = "ChallengeMode_VoidRaidCrabEXBodySecondaryFamily";
			skillFamilySecondary.variants = new SkillFamily.Variant[]
			{
				new SkillFamily.Variant
				{
					skillDef = skillDirectLasers
				},
				new SkillFamily.Variant
				{
					skillDef = skillReaverBombPattern
				},
				new SkillFamily.Variant
				{
					skillDef = skillDevastatorBombMortar
				}
			};
			skillFamilySecondary.defaultVariantIndex = 0;
			ChallengeModeContent.Resources.skillFamilies.Add(skillFamilySecondary);

			skillKnockUpEveryone = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillKnockUpEveryone).name = "ChallengeMode_VoidRaidCrabEXBodyKnockUpEveryone";
			skillKnockUpEveryone.skillName = "KnockUpEveryone";
			skillKnockUpEveryone.activationStateMachineName = "Weapon";
			skillKnockUpEveryone.activationState = new SerializableEntityStateType(typeof(ChargeKnockUp));
			skillKnockUpEveryone.interruptPriority = InterruptPriority.Skill;
			skillKnockUpEveryone.baseRechargeInterval = 28f;
			skillKnockUpEveryone.baseMaxStock = 1;
			skillKnockUpEveryone.rechargeStock = 1;
			skillKnockUpEveryone.requiredStock = 1;
			skillKnockUpEveryone.stockToConsume = 1;
			skillKnockUpEveryone.resetCooldownTimerOnUse = true;
			skillKnockUpEveryone.fullRestockOnAssign = true;
			skillKnockUpEveryone.dontAllowPastMaxStocks = true;
			skillKnockUpEveryone.beginSkillCooldownOnSkillEnd = false;
			skillKnockUpEveryone.cancelSprintingOnActivation = true;
			skillKnockUpEveryone.forceSprintDuringState = false;
			skillKnockUpEveryone.canceledFromSprinting = false;
			skillKnockUpEveryone.isCombatSkill = true;
			skillKnockUpEveryone.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillKnockUpEveryone);
			ChargeKnockUp.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabMuzzleflashEyeMissiles.prefab").WaitForCompletion();
			KnockUpEveryone.blastEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RoboBallBoss/RoboBallBossDelayKnockupEffect.prefab").WaitForCompletion();
			
			skillVacuum = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidRaidCrab/RaidCrabVacuumAttack.asset").WaitForCompletion();

			skillReaverBombSnake = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillReaverBombSnake).name = "ChallengeMode_VoidRaidCrabEXBodyReaverBombSnake";
			skillReaverBombSnake.skillName = "ReaverBombSnake";
			skillReaverBombSnake.activationStateMachineName = "Weapon";
			skillReaverBombSnake.activationState = new SerializableEntityStateType(typeof(FireReaverBombSnake));
			skillReaverBombSnake.interruptPriority = InterruptPriority.Skill;
			skillReaverBombSnake.baseRechargeInterval = 25f;
			skillReaverBombSnake.baseMaxStock = 1;
			skillReaverBombSnake.rechargeStock = 1;
			skillReaverBombSnake.requiredStock = 1;
			skillReaverBombSnake.stockToConsume = 1;
			skillReaverBombSnake.resetCooldownTimerOnUse = true;
			skillReaverBombSnake.fullRestockOnAssign = true;
			skillReaverBombSnake.dontAllowPastMaxStocks = true;
			skillReaverBombSnake.beginSkillCooldownOnSkillEnd = false;
			skillReaverBombSnake.cancelSprintingOnActivation = true;
			skillReaverBombSnake.forceSprintDuringState = false;
			skillReaverBombSnake.canceledFromSprinting = false;
			skillReaverBombSnake.isCombatSkill = true;
			skillReaverBombSnake.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillReaverBombSnake);
			FireReaverBombSnake.muzzleFlashPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabFinalStandMuzzleflash.prefab").WaitForCompletion();
			FireReaverBombSnake.ChallengeModeVoidRaidCrabEXSnake.projectilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierDeathBombProjectile.prefab").WaitForCompletion(), "ChallengeMode_VoidRaidCrabEXMiniReaverBomb", true);
			var miniBombScale = 7f;
			FireReaverBombSnake.ChallengeModeVoidRaidCrabEXSnake.projectilePrefab.GetComponent<ProjectileExplosion>().blastRadius = miniBombScale;
			FireReaverBombSnake.ChallengeModeVoidRaidCrabEXSnake.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab = PrefabAPI.InstantiateClone(FireReaverBombSnake.ChallengeModeVoidRaidCrabEXSnake.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab, "ChallengeMode_VoidRaidCrabEXMiniReaverBombGhost", false);
			FireReaverBombSnake.ChallengeModeVoidRaidCrabEXSnake.projectilePrefab.GetComponent<ProjectileController>().ghostPrefab.transform.Find("Scale").localScale = Vector3.one * miniBombScale;

			skillFamilyUtility = ScriptableObject.CreateInstance<SkillFamily>();
			((ScriptableObject)skillFamilyUtility).name = "ChallengeMode_VoidRaidCrabEXBodyUtilityFamily";
			skillFamilyUtility.variants = new SkillFamily.Variant[]
			{
				new SkillFamily.Variant
				{
					skillDef = skillKnockUpEveryone
				},
				new SkillFamily.Variant
				{
					skillDef = skillVacuum
				},
				new SkillFamily.Variant
				{
					skillDef = skillReaverBombSnake
				}
			};
			skillFamilyUtility.defaultVariantIndex = 0;
			ChallengeModeContent.Resources.skillFamilies.Add(skillFamilyUtility);

			skillScream = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillScream).name = "ChallengeMode_VoidRaidCrabEXBodyScream";
			skillScream.skillName = "Scream";
			skillScream.activationStateMachineName = "Body";
			skillScream.activationState = new SerializableEntityStateType(typeof(EnterScream));
			skillScream.interruptPriority = InterruptPriority.Skill;
			skillScream.baseRechargeInterval = 60f;
			skillScream.baseMaxStock = 1;
			skillScream.rechargeStock = 1;
			skillScream.requiredStock = 1;
			skillScream.stockToConsume = 1;
			skillScream.resetCooldownTimerOnUse = true;
			skillScream.fullRestockOnAssign = true;
			skillScream.dontAllowPastMaxStocks = true;
			skillScream.beginSkillCooldownOnSkillEnd = false;
			skillScream.cancelSprintingOnActivation = false;
			skillScream.forceSprintDuringState = false;
			skillScream.canceledFromSprinting = false;
			skillScream.isCombatSkill = false;
			skillScream.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillScream);
			Scream.loopSoundDef = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/VoidRaidCrab/lsdVoidRaidCrabSpinBeam.asset").WaitForCompletion();
			Scream.screamFXPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/VoidRaidCrabScreamFX.prefab");
			var shakeEmitter = Scream.screamFXPrefab.AddComponent<ShakeEmitter>();
			shakeEmitter.duration = 999f;
			shakeEmitter.wave = new Wave
			{
				amplitude = 0.14f,
				frequency = 9f
			};
			shakeEmitter.shakeOnStart = true;
			shakeEmitter.shakeOnEnable = false;
			shakeEmitter.radius = 9999f;
			shakeEmitter.scaleShakeRadiusWithLocalScale = false;
			shakeEmitter.amplitudeTimeDecay = false;

			skillScreamCallForHelp = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillScreamCallForHelp).name = "ChallengeMode_VoidRaidCrabEXBodyScreamCallForHelp";
			skillScreamCallForHelp.skillName = "ScreamCallForHelp";
			skillScreamCallForHelp.activationStateMachineName = "Body";
			skillScreamCallForHelp.activationState = new SerializableEntityStateType(typeof(EnterScreamCallForHelp));
			skillScreamCallForHelp.interruptPriority = InterruptPriority.Skill;
			skillScreamCallForHelp.baseRechargeInterval = 120f;
			skillScreamCallForHelp.baseMaxStock = 1;
			skillScreamCallForHelp.rechargeStock = 1;
			skillScreamCallForHelp.requiredStock = 1;
			skillScreamCallForHelp.stockToConsume = 1;
			skillScreamCallForHelp.resetCooldownTimerOnUse = true;
			skillScreamCallForHelp.fullRestockOnAssign = true;
			skillScreamCallForHelp.dontAllowPastMaxStocks = true;
			skillScreamCallForHelp.beginSkillCooldownOnSkillEnd = false;
			skillScreamCallForHelp.cancelSprintingOnActivation = false;
			skillScreamCallForHelp.forceSprintDuringState = false;
			skillScreamCallForHelp.canceledFromSprinting = false;
			skillScreamCallForHelp.isCombatSkill = false;
			skillScreamCallForHelp.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillScreamCallForHelp);
			ScreamCallForHelp.helperSpawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Nullifier/cscNullifier.asset").WaitForCompletion();

			skillBeamSweep = ScriptableObject.CreateInstance<SkillDef>();
			((ScriptableObject)skillBeamSweep).name = "ChallengeMode_VoidRaidCrabEXBodyBeamSweep";
			skillBeamSweep.skillName = "BeamSweep";
			skillBeamSweep.activationStateMachineName = "Body";
			skillBeamSweep.activationState = new SerializableEntityStateType(typeof(EnterBeamSweepTurning));
			skillBeamSweep.interruptPriority = InterruptPriority.Skill;
			skillBeamSweep.baseRechargeInterval = 60f;
			skillBeamSweep.baseMaxStock = 1;
			skillBeamSweep.rechargeStock = 1;
			skillBeamSweep.requiredStock = 1;
			skillBeamSweep.stockToConsume = 1;
			skillBeamSweep.resetCooldownTimerOnUse = true;
			skillBeamSweep.fullRestockOnAssign = true;
			skillBeamSweep.dontAllowPastMaxStocks = true;
			skillBeamSweep.beginSkillCooldownOnSkillEnd = false;
			skillBeamSweep.cancelSprintingOnActivation = false;
			skillBeamSweep.forceSprintDuringState = false;
			skillBeamSweep.canceledFromSprinting = false;
			skillBeamSweep.isCombatSkill = false;
			skillBeamSweep.mustKeyPress = false;
			ChallengeModeContent.Resources.skillDefs.Add(skillBeamSweep);
			BaseFireBeamSweep.loopSoundDef = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/VoidRaidCrab/lsdVoidRaidCrabSpinBeam.asset").WaitForCompletion();
			FireBeamSweepTurning.loopSoundDef = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/VoidRaidCrab/lsdVoidRaidCrabSpinBeam.asset").WaitForCompletion();

			skillFamilySpecial = ScriptableObject.CreateInstance<SkillFamily>();
			((ScriptableObject)skillFamilySpecial).name = "ChallengeMode_VoidRaidCrabEXBodySpecialFamily";
			skillFamilySpecial.variants = new SkillFamily.Variant[]
			{
				new SkillFamily.Variant
				{
					skillDef = skillScream
				},
				new SkillFamily.Variant
				{
					skillDef = skillScreamCallForHelp
				},
				new SkillFamily.Variant
				{
					skillDef = skillBeamSweep
				}
			};
			skillFamilySpecial.defaultVariantIndex = 0;
			ChallengeModeContent.Resources.skillFamilies.Add(skillFamilySpecial);

			GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;

            OnLoad();
            asset = new GameObject();
        }

        private static void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.damageInfo.procCoefficient > 0 && NetworkServer.active && DamageAPI.HasModdedDamageType(damageReport.damageInfo, disablePrimaryDamageType) && damageReport.victimBody)
            {
                damageReport.victimBody.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_Disarmed, 4f * damageReport.damageInfo.procCoefficient);
            }
        }

		public class ChallengeModeDisarmingProjectile : MonoBehaviour, IOnDamageInflictedServerReceiver
		{
			public void OnDamageInflictedServer(DamageReport damageReport)
			{
				var victimBody = damageReport.victimBody;
				if (victimBody)
				{
					victimBody.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_Disarmed, 4f * damageReport.damageInfo.procCoefficient);
				}
			}
		}

		public static Vector3 GetActiveDonutPosition(Vector3 defaultIfNotFound)
        {
			var donutPosition = defaultIfNotFound;
			if (VoidRaidGauntletController.instance && VoidRaidGauntletController.instance.currentDonut != null)
			{
				var donutRoot = VoidRaidGauntletController.instance.currentDonut.root;
				if (donutRoot)
				{
					donutPosition = donutRoot.transform.position;
					donutPosition.x = Mathf.Round(donutPosition.x / 1000f) * 1000f;
					donutPosition.y = 0f;
					donutPosition.z = Mathf.Round(donutPosition.z / 1000f) * 1000f;
				}
			}
			return donutPosition;
		}

		public class FireDisarmingOrb : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			public static GameObject projectilePrefab;

			public static float baseDuration = 0.12f;
			public static float baseDurationLastFired = 2f;

			public float duration;
			public Transform muzzle;

			public float damageCoefficient = 0.3f;
			public float force = 100f;
			public float coneSpreadAngle = 6.5f;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				if (skillLocator.primaryBonusStockSkill.stock <= 0)
				{
					duration = baseDurationLastFired / attackSpeedStat;
				}
				muzzle = FindModelChild("EyeProjectileCenter");
				if (!muzzle) muzzle = characterBody.coreTransform;

				PlayAnimation("Gesture", "FireEyeBlast", "Eyeblast.playbackRate", duration);
				EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, "EyeProjectileCenter", false);

				if (isAuthority)
				{
					var aimRotation = Util.QuaternionSafeLookRotation(GetAimRay().direction);
					var fireProjectileInfo = new FireProjectileInfo
					{
						projectilePrefab = projectilePrefab,
						position = muzzle.position,
						owner = gameObject,
						damage = damageStat * damageCoefficient,
						force = force,
						crit = characterBody.RollCrit(),
						rotation = aimRotation * GetRandomRollPitch()
					};
					ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public Quaternion GetRandomRollPitch()
			{
				return Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Quaternion.AngleAxis(Random.Range(0f, coneSpreadAngle), Vector3.left);
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class ChargeAheadLaser : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			public static GameObject laserPrefab;

			public static float baseDuration = 2f;
			public static float laserMaxWidth = 0.6f;
			public static string muzzleFlashSoundString = "Play_voidRaid_snipe_chargeUp";
			public static float muzzleFlashDuration = 0.3f;
			
			public float duration;

			public BullseyeSearch enemyFinder;
			public HurtBox lockedOnHurtBox;
			public CharacterBody lockedOnBody;
			public GameObject chargeEffect;
			public GameObject laserEffect;
			public LineRenderer laserLineComponent;

			public float flashTimer = 0f;
			public bool laserOn = true;

			public Transform muzzle;
			public bool muzzleFlashed = false;

			public Ray currentRay;
			public Vector3 currentRayEndPos;
			public float aimVelocity;
			public float aimVectorDampTime = 0.1f;
			public float aimVectorMaxSpeed = 20f;
			public float aheadFactor = 1.16f;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;

				var aimRay = GetAimRay();
				enemyFinder = new BullseyeSearch();
				enemyFinder.maxDistanceFilter = 2000f;
				enemyFinder.searchOrigin = aimRay.origin;
				enemyFinder.searchDirection = aimRay.direction;
				enemyFinder.filterByLoS = false;
				enemyFinder.sortMode = BullseyeSearch.SortMode.Angle;
				enemyFinder.teamMaskFilter = TeamMask.allButNeutral;
				if (teamComponent) enemyFinder.teamMaskFilter.RemoveTeam(teamComponent.teamIndex);
				enemyFinder.RefreshCandidates();
				lockedOnHurtBox = enemyFinder.GetResults().FirstOrDefault();

				var modelChildLocator = GetModelChildLocator();
				if (modelChildLocator)
				{
					muzzle = modelChildLocator.FindChild("EyeProjectileCenter");
					if (!muzzle) muzzle = characterBody.coreTransform;
					if (laserPrefab)
					{
						laserEffect = Object.Instantiate(laserPrefab, muzzle.position, muzzle.rotation);
						laserEffect.transform.parent = muzzle;
						laserLineComponent = laserEffect.GetComponent<LineRenderer>();
					}
				}
				if (characterBody) characterBody.SetAimTimer(duration);

				currentRay = GetAimRay();
				if (lockedOnHurtBox &&
					lockedOnHurtBox.hurtBoxGroup &&
					lockedOnHurtBox.hurtBoxGroup.mainHurtBox &&
					lockedOnHurtBox.hurtBoxGroup.mainHurtBox.healthComponent &&
					lockedOnHurtBox.hurtBoxGroup.mainHurtBox.healthComponent.body)
                {
					lockedOnBody = lockedOnHurtBox.hurtBoxGroup.mainHurtBox.healthComponent.body;
					if (lockedOnBody.isFlying) aheadFactor = 0f;
				}
			}

			public override void Update()
			{
				base.Update();
				if (laserEffect && laserLineComponent)
				{
					var maxDistance = 1000f;
					
					var aimRay = currentRay;
					if (lockedOnHurtBox)
					{
						var velocity = Vector3.zero;
						if (lockedOnBody)
                        {
							if (lockedOnBody.characterMotor)
								velocity += lockedOnBody.characterMotor.velocity;
							if (lockedOnBody.rigidbody)
								velocity += lockedOnBody.rigidbody.velocity;
						}

						velocity.y = 0f;

						var desiredAimDirection = (lockedOnHurtBox.transform.position + velocity * aheadFactor - aimRay.origin).normalized;
						var targetRotation = Util.QuaternionSafeLookRotation(desiredAimDirection);
						aimRay.direction = Util.SmoothDampQuaternion(
							Util.QuaternionSafeLookRotation(aimRay.direction),
							targetRotation,
							ref aimVelocity,
							aimVectorDampTime,
							aimVectorMaxSpeed,
							Time.deltaTime
						) * Vector3.forward;
					}

					var eyePosition = laserEffect.transform.parent.position;
					var targetPosition = aimRay.GetPoint(maxDistance);
					if (Physics.Raycast(aimRay, out var raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.defaultLayer.mask, QueryTriggerInteraction.Ignore))
					{
						targetPosition = raycastHit.point;
					}
					laserLineComponent.SetPosition(0, eyePosition);
					laserLineComponent.SetPosition(1, targetPosition);

					currentRay = aimRay;
					currentRayEndPos = targetPosition;

					var laserWidth = 0f;
					if (duration - age > 0.5f)
					{
						laserWidth = age / duration;
					}
					else
					{
						flashTimer -= Time.deltaTime;
						if (flashTimer <= 0f)
						{
							laserOn = !laserOn;
							flashTimer = 0.0333333351f;
						}
						laserWidth = laserOn ? 1f : 0f;

						if (!muzzleFlashed)
						{
							muzzleFlashed = true;
							Util.PlaySound(muzzleFlashSoundString, gameObject);

							if (muzzle)
							{
								if (muzzleFlashPrefab)
								{
									chargeEffect = Object.Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation);
									chargeEffect.transform.parent = muzzle;
									var scaleParticleSystemDuration = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
									if (scaleParticleSystemDuration)
										scaleParticleSystemDuration.newDuration = muzzleFlashDuration;
								}
							}
						}
					}
					laserWidth *= laserMaxWidth;
					laserLineComponent.startWidth = laserWidth;
					laserLineComponent.endWidth = laserWidth;
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (fixedAge >= duration && isAuthority)
				{
					outer.SetNextState(new FireAheadLaser
                    {
						forcedRay = currentRay,
						forcedBeamEndPos = currentRayEndPos
                    });
					return;
				}
			}

			public override void OnExit()
			{
				base.OnExit();
				if (chargeEffect) Destroy(chargeEffect);
				if (laserEffect) Destroy(laserEffect);
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class FireAheadLaser : EntityStates.VoidRaidCrab.Weapon.FireMultiBeamFinale
		{
			public static GameObject _muzzleEffectPrefab;
			public static GameObject _tracerEffectPrefab;
			public static GameObject _explosionEffectPrefab;
			public static GameObject _projectilePrefab;

			public Ray forcedRay;
			public Vector3 forcedBeamEndPos;

			public override void OnEnter()
			{
				baseDuration = 0.4f;
				animationLayerName = "Gesture";
				animationStateName = "FireMultiBeamFinale";
				animationPlaybackRateParam = "MultiBeam.playbackRate";
				enterSoundString = "Play_voidRaid_snipe_shoot_final";
				muzzleEffectPrefab = _muzzleEffectPrefab;
				tracerEffectPrefab = _tracerEffectPrefab;
				explosionEffectPrefab = _explosionEffectPrefab;
				projectilePrefab = _projectilePrefab;
				projectileDamageCoefficient = 1f;
				blastDamageCoefficient = 3f;
				blastForceMagnitude = 5000f;
				blastRadius = 6f;
				blastBonusForce = Vector3.up * 100f;
                On.EntityStates.VoidRaidCrab.Weapon.BaseMultiBeamState.CalcBeamPath += BaseMultiBeamState_CalcBeamPath;
				base.OnEnter();
				On.EntityStates.VoidRaidCrab.Weapon.BaseMultiBeamState.CalcBeamPath -= BaseMultiBeamState_CalcBeamPath;
			}

            private void BaseMultiBeamState_CalcBeamPath(On.EntityStates.VoidRaidCrab.Weapon.BaseMultiBeamState.orig_CalcBeamPath orig, EntityStates.VoidRaidCrab.Weapon.BaseMultiBeamState self, out Ray beamRay, out Vector3 beamEndPos)
            {
				orig(self, out beamRay, out beamEndPos);
				beamRay = forcedRay;
				beamEndPos = forcedBeamEndPos;
            }
        }

		public class FireDevastatorMortar : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			public static GameObject projectilePrefab;
			public static GameObject warningEffectPrefab;

			public static float baseDuration = 3.2f;

			public float duration;
			public Transform muzzle;

			public int projectileCount = 5;
			public float coneSpreadAngle = 20f;
			public float maxDistance = 500f;
			public float timeToTarget = 2.5f;
			public float defaultLaunchMagnitude = 150f;
			public float horizontalCorrection = 0.952f;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				muzzle = FindModelChild("EyeProjectileCenter");
				if (!muzzle) muzzle = characterBody.coreTransform;

				PlayAnimation("Gesture", "FireEyeBlast", "Eyeblast.playbackRate", duration);
				EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, "EyeProjectileCenter", false);

				if (isAuthority)
				{
					for (var i = 0; i < projectileCount; i++)
					{
						var aimRay = GetAimRay();
						aimRay.origin = muzzle.position;
						var endPoint = aimRay.GetPoint(maxDistance);
						var endPointNormal = -aimRay.direction.normalized;
						var launchMagnitude = defaultLaunchMagnitude;

						var randomizeAttempts = 20;
						for (var j = 0; j < randomizeAttempts; j++)
						{
							var randomizedRay = GetAimRay();
							randomizedRay.direction = (Util.QuaternionSafeLookRotation(randomizedRay.direction) * GetRandomRollPitch()) * Vector3.forward;
							if (Util.CharacterRaycast(gameObject, randomizedRay, out var raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
							{
								endPoint = raycastHit.point;
								endPointNormal = raycastHit.normal;
								var distanceToEndPoint = endPoint - randomizedRay.origin;

								var horizontalMovement = new Vector2(distanceToEndPoint.x, distanceToEndPoint.z);
								var horizontalMagnitude = horizontalMovement.magnitude;
								var horizontalNormalized = horizontalMovement / horizontalMagnitude;

								var finalHorizontalSpeed = horizontalMagnitude / timeToTarget * horizontalCorrection;
								var finalVerticalSpeed = Trajectory.CalculateInitialYSpeed(timeToTarget, distanceToEndPoint.y);

								var finalDirection = new Vector3(horizontalNormalized.x * finalHorizontalSpeed, finalVerticalSpeed, horizontalNormalized.y * finalHorizontalSpeed);
								launchMagnitude = finalDirection.magnitude;
								aimRay.direction = finalDirection;

								break;
							}
						}

						if (NetworkServer.active)
						{
							if (warningEffectPrefab) EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
							{
								origin = endPoint,
								rotation = Util.QuaternionSafeLookRotation(endPointNormal)
							}, true);
                        }
                        else
                        {
							new RequestServerSpawnWarningEffect(endPoint, Util.QuaternionSafeLookRotation(endPointNormal)).Send(NetworkDestination.Clients);
						}

						var aimRotation = Util.QuaternionSafeLookRotation(aimRay.direction);
						var fireProjectileInfo = new FireProjectileInfo
						{
							projectilePrefab = projectilePrefab,
							position = aimRay.origin,
							owner = gameObject,
							damage = damageStat,
							crit = characterBody.RollCrit(),
							rotation = aimRotation,
							speedOverride = launchMagnitude
						};
						ProjectileManager.instance.FireProjectile(fireProjectileInfo);
					}
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public Quaternion GetRandomRollPitch()
			{
				return Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.forward) * Quaternion.AngleAxis(Random.Range(0f, coneSpreadAngle), Vector3.left);
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}

			public class RequestServerSpawnWarningEffect : INetMessage
			{
				Vector3 origin;
				Quaternion rotation;

				public RequestServerSpawnWarningEffect()
				{
				}

				public RequestServerSpawnWarningEffect(Vector3 origin, Quaternion rotation)
				{
					this.origin = origin;
					this.rotation = rotation;
				}

				public void Deserialize(NetworkReader reader)
				{
					origin = reader.ReadVector3();
					rotation = reader.ReadQuaternion();
				}

				public void OnReceived()
				{
					if (!NetworkServer.active) return;
					if (warningEffectPrefab) EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
					{
						origin = origin,
						rotation = rotation
					}, true);
				}

				public void Serialize(NetworkWriter writer)
				{
					writer.Write(origin);
					writer.Write(rotation);
				}
			}
		}

		public class ChargeDirectLasers : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			public static GameObject laserPrefab;
			
			public static float baseDuration = 5f;
			public static float laserMaxWidth = 0.4f;
			public static float lockOnAngle = 30f;
			public static string muzzleFlashSoundString = "Play_voidRaid_snipe_chargeUp";
			public static float muzzleFlashDuration = 0.3f;

			public float duration;

			public GameObject chargeEffect;
			public GameObject laserEffect;
			public LineRenderer laserLineComponent;
			
			public float flashTimer = 0f;
			public bool laserOn = true;

			public Transform muzzle;
			public bool muzzleFlashed = false;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;

				var modelChildLocator = GetModelChildLocator();
				if (modelChildLocator)
				{
					muzzle = modelChildLocator.FindChild("EyeProjectileCenter");
					if (!muzzle) muzzle = characterBody.coreTransform;
					if (laserPrefab)
					{
						laserEffect = Object.Instantiate(laserPrefab, muzzle.position, muzzle.rotation);
						laserEffect.transform.parent = muzzle;
						laserLineComponent = laserEffect.GetComponent<LineRenderer>();
					}
				}
				if (characterBody) characterBody.SetAimTimer(duration);
			}

			public override void Update()
			{
				base.Update();
				if (laserEffect && laserLineComponent)
				{
					var maxDistance = 1000f;

					var aimRay = GetAimRay();
					var eyePosition = laserEffect.transform.parent.position;
					var targetPosition = aimRay.GetPoint(maxDistance);
					if (Physics.Raycast(aimRay, out var raycastHit, maxDistance, LayerIndex.world.mask | LayerIndex.defaultLayer.mask))
					{
						targetPosition = raycastHit.point;
					}
					laserLineComponent.SetPosition(0, eyePosition);
					laserLineComponent.SetPosition(1, targetPosition);

					var laserWidth = 0f;
					if (duration - age > 0.5f)
					{
						laserWidth = age / duration;
					}
					else
					{
						flashTimer -= Time.deltaTime;
						if (flashTimer <= 0f)
						{
							laserOn = !laserOn;
							flashTimer = 0.0333333351f;
						}
						laserWidth = laserOn ? 1f : 0f;

						if (!muzzleFlashed)
                        {
							muzzleFlashed = true;
							Util.PlaySound(muzzleFlashSoundString, gameObject);
							
							if (muzzle)
							{
								if (muzzleFlashPrefab)
								{
									chargeEffect = Object.Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation);
									chargeEffect.transform.parent = muzzle;
									var scaleParticleSystemDuration = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
									if (scaleParticleSystemDuration)
										scaleParticleSystemDuration.newDuration = muzzleFlashDuration;
								}
							}
						}
					}
					laserWidth *= laserMaxWidth;
					laserLineComponent.startWidth = laserWidth;
					laserLineComponent.endWidth = laserWidth;
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (fixedAge >= duration && isAuthority)
				{
					outer.SetNextState(new FireDirectLasers());
					return;
				}
			}

			public override void OnExit()
			{
				base.OnExit();
				if (chargeEffect) Destroy(chargeEffect);
				if (laserEffect) Destroy(laserEffect);
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class FireDirectLasers : EntityStates.VoidRaidCrab.Weapon.FireMultiBeamSmall
		{
			public int totalLasers = 3;

			public static GameObject _muzzleEffectPrefab;
			public static GameObject _tracerEffectPrefab;
			public static GameObject _explosionEffectPrefab;

			public override void OnEnter()
			{
				baseDuration = 0.4f;
				animationLayerName = "Gesture";
				animationStateName = "FireMultiBeamSmall";
				animationPlaybackRateParam = "MultiBeam.playbackRate";
				enterSoundString = "Play_voidRaid_snipe_shoot";
				muzzleEffectPrefab = _muzzleEffectPrefab;
				tracerEffectPrefab = _tracerEffectPrefab;
				explosionEffectPrefab = _explosionEffectPrefab;
				blastDamageCoefficient = 2f;
				blastForceMagnitude = 3000f;
				blastRadius = 6f;
				blastBonusForce = Vector3.up * 100f;
				base.OnEnter();
            }

            public override EntityState InstantiateNextState()
			{
				if (fireIndex < totalLasers - 1)
				{
					return new FireDirectLasers
					{
						fireIndex = fireIndex + 1
					};
				}
				return EntityStateCatalog.InstantiateState(outer.mainStateType);
			}
		}

		public class FireReaverBombPattern : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			public static GameObject projectilePrefab;

			public float baseDuration = 3f;
			public float patternRadius = 150f + 25f;
			public int bombLineCount = 12;
			public int bombCountInLine = 3;
			public float bombOffsetInLine = 25f;
			public static float globalInitialRotation = 0f;
			
			public float duration;
			public Vector3 patternRootPosition;
			public bool useGlobalInitialRotation = true;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration;
				EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, "EyeProjectileCenter", false);
				Util.PlaySound("Play_voidRaid_m1_shoot", gameObject);
				PlayAnimation("Gesture", "FireEyeBlast", "Eyeblast.playbackRate", duration);

				patternRootPosition = GetActiveDonutPosition(transform.position);

				var rotationPerBomb = 360f / (float)bombLineCount;
				var initialRotation = 0f;
				if (useGlobalInitialRotation)
				{
					initialRotation = globalInitialRotation;
					globalInitialRotation += rotationPerBomb / 2f;
				}
				else initialRotation = RoR2Application.rng.RangeFloat(0f, 360f);
				for (var i = 0; i < bombLineCount; i++)
                {
					var lineForward = Quaternion.AngleAxis(initialRotation + rotationPerBomb * i, Vector3.up) * Vector3.forward;
					for (var j = 0; j < bombCountInLine; j++)
					{
						var bombPosition = patternRootPosition + lineForward * patternRadius;
						bombPosition -= lineForward * bombOffsetInLine * j;

						var fireProjectileInfo = new FireProjectileInfo
						{
							projectilePrefab = projectilePrefab,
							position = bombPosition,
							rotation = Quaternion.identity,
							owner = gameObject,
							damage = damageStat,
							crit = characterBody.RollCrit()
						};
						ProjectileManager.instance.FireProjectile(fireProjectileInfo);
					}
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class ChargeKnockUp : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			public static GameObject preBlastEffect;

			public static float baseDuration = 2f;
			public static float attackSpeedSoundScale = 0.54f;
			public GameObject beamVfxInstance;

			public float duration;
			
			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, "EyeProjectileCenter", false);
				Util.PlayAttackSpeedSound("Play_voidRaid_snipe_chargeUp", gameObject, attackSpeedStat * attackSpeedSoundScale);
				CreateBeamVFXInstance(EntityStates.VoidRaidCrab.SpinBeamWindUp.warningLaserPrefab);

				var myTeamIndex = GetTeam();
				for (var teamIndex = 0; teamIndex < TeamCatalog.teamDefs.Length; teamIndex++)
				{
					if ((TeamIndex)teamIndex != myTeamIndex)
					{
						foreach (var enemy in TeamComponent.GetTeamMembers((TeamIndex)teamIndex))
						{
							if (enemy.body)
							{
								if (NetworkServer.active && preBlastEffect)
								{
									var ray = new Ray(enemy.body.footPosition, Vector3.down);
									var maxDistance = 100f;
									var groundPoint = ray.GetPoint(maxDistance);
									if (Physics.Raycast(ray, out var hitInfo, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
										groundPoint = hitInfo.point;

									EffectManager.SpawnEffect(preBlastEffect, new EffectData
									{
										origin = groundPoint,
										scale = 8f
									}, true);
								}
							}
						}
					}
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextState(new KnockUpEveryone());
				}
			}

			public void CreateBeamVFXInstance(GameObject beamVfxPrefab)
			{
				if (beamVfxInstance == null)
				{
					beamVfxInstance = Object.Instantiate(beamVfxPrefab);
					beamVfxInstance.transform.SetParent(FindModelChild(EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.headTransformNameInChildLocator), true);
					UpdateBeamTransforms();
					RoR2Application.onLateUpdate += UpdateBeamTransformsInLateUpdate;
				}
			}

			public void DestroyBeamVFXInstance()
			{
				if (beamVfxInstance != null)
				{
					RoR2Application.onLateUpdate -= UpdateBeamTransformsInLateUpdate;
					VfxKillBehavior.KillVfxObject(beamVfxInstance);
					beamVfxInstance = null;
				}
			}

			public void UpdateBeamTransformsInLateUpdate()
			{
				try
				{
					UpdateBeamTransforms();
				}
				catch { }
			}

			private void UpdateBeamTransforms()
			{
				var beamRay = GetAimRay();
				beamVfxInstance.transform.SetPositionAndRotation(beamRay.origin, Quaternion.LookRotation(beamRay.direction));
			}

			public override void OnExit()
			{
				DestroyBeamVFXInstance();
				base.OnExit();
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class KnockUpEveryone : BaseState
		{
			public static GameObject blastEffect;

			public static float baseDuration = 1.5f;

			public float duration;

			public float force = 7000f;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				PlayAnimation("Gesture", "FireMultiBeamFinale", "MultiBeam.playbackRate", duration);

				var myTeamIndex = GetTeam();
				for (var teamIndex = 0; teamIndex < TeamCatalog.teamDefs.Length; teamIndex++)
				{
					if ((TeamIndex)teamIndex != myTeamIndex)
					{
						foreach (var enemy in TeamComponent.GetTeamMembers((TeamIndex)teamIndex))
						{
							if (enemy.body)
							{
								Util.PlaySound("Play_roboBall_attack3_gravityBump_explo", enemy.gameObject);
								if (NetworkServer.active && blastEffect)
								{
									var ray = new Ray(enemy.body.footPosition, Vector3.down);
									var maxDistance = 100f;
									var groundPoint = ray.GetPoint(maxDistance);
									if (Physics.Raycast(ray, out var hitInfo, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
										groundPoint = hitInfo.point;

									EffectManager.SpawnEffect(blastEffect, new EffectData
									{
										origin = groundPoint,
										scale = 40f
									}, true);
                                }
								Buffs.PlayerKnockupStun.KnockupBody(enemy.body, Vector3.up * force);
							}
						}
					}
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

            public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class FireReaverBombSnake : BaseState
		{
			public static GameObject muzzleFlashPrefab;
			
			public float baseDuration = 1f;
			public float duration;
			
			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration;
				EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, "EyeProjectileCenter", false);
				Util.PlaySound("Play_voidRaid_snipe_chargeUp", gameObject);
				
				if (isAuthority)
                {
					var snakeSpawner = new GameObject();
					var snakeRootPosition = GetActiveDonutPosition(transform.position);
					snakeSpawner.transform.position = snakeRootPosition;
					var snakeComponent = snakeSpawner.AddComponent<ChallengeModeVoidRaidCrabEXSnake>();
					snakeComponent.owner = gameObject;
				}
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}

			public class ChallengeModeVoidRaidCrabEXSnake : MonoBehaviour
            {
				public static GameObject projectilePrefab;

				public GameObject owner;
				public float angleSpeed = 4.5f;
				public float currentAngle = 0f;
				public float centerDistance = 150f;
				public float timer = 0f;
				public float interval = 0.1f;
				public float sineT = 0f;
				public float sineFrequency = 2f;
				public float sineAmplitude = 15f;
				public int bombsSpawned = 0;
				public int bombLimit = 100;

				public void Awake()
                {
					currentAngle = RoR2Application.rng.RangeFloat(0f, 360f);
					if (Util.CheckRoll(50f)) angleSpeed = -angleSpeed;
				}

				public void FixedUpdate()
                {
					if (owner && projectilePrefab)
					{
						timer -= Time.fixedDeltaTime;
						sineT += Time.fixedDeltaTime;
						if (timer <= 0)
						{
							timer += interval;

							var lineForward = Quaternion.AngleAxis(currentAngle, Vector3.up) * Vector3.forward;
							var bombPosition = transform.position + lineForward * (centerDistance + sineAmplitude * Mathf.Sin(sineT * sineFrequency * Mathf.PI));
							
							var fireProjectileInfo = new FireProjectileInfo
							{
								projectilePrefab = projectilePrefab,
								position = bombPosition,
								rotation = Quaternion.identity,
								owner = gameObject,
								damage = 1f,
								crit = false
							};
							ProjectileManager.instance.FireProjectile(fireProjectileInfo);

							currentAngle += angleSpeed;
							bombsSpawned++;
							if (bombsSpawned >= bombLimit) Destroy(gameObject);
						}
					}
                }
            }
		}

		public class EnterScream : BaseState
		{
			public static float baseDuration = 2f;
			
			public float duration;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				PlayAnimation("Body", "SuckEnter", "Suck.playbackRate", duration);
				Util.PlayAttackSpeedSound("Play_voidRaid_superLaser_chargeUp", gameObject, attackSpeedStat);
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextState(new Scream());
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class Scream : BaseState
		{
			public static LoopSoundDef loopSoundDef;
			public static GameObject screamFXPrefab;

			public float baseDuration = 6f;
			public float duration;
			public LoopSoundManager.SoundLoopPtr loopPtr;
			public float tickTimer;
			public float tickFrequency = 0.5f;
			public Transform pushAwayOrigin;
			public float pushAwayPower = 7f;
			public float debuffDuration = 12f;
			public GameObject screamFX;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				PlayAnimation("Body", "SuckLoop", "Suck.playbackRate", duration);
				Util.PlaySound("Play_voidRaid_superLaser_start", gameObject);
				if (loopSoundDef) loopPtr = LoopSoundManager.PlaySoundLoopLocal(gameObject, loopSoundDef);
				var childLocator = GetModelChildLocator();
				if (childLocator)
                {
					pushAwayOrigin = childLocator.FindChild("Head");
					if (pushAwayOrigin)
                    {
						if (screamFXPrefab)
                        {
							screamFX = Object.Instantiate(screamFXPrefab, pushAwayOrigin);
                        }
					}
                }

				PostProcessing.ChallengeModePostProcessing.voidRaidCrabScreamIntensityTarget = 1f;
			}

            public override void Update()
            {
                base.Update();
				if (characterBody)
					PostProcessing.ChallengeModePostProcessing.voidRaidCrabWorldPosition = characterBody.corePosition;
			}

            public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextState(new ExitScream());
				}

				var tickTriggered = false;
				tickTimer -= Time.fixedDeltaTime;
				if (tickTimer <= 0)
				{
					tickTimer += tickFrequency;
					tickTriggered = true;
					OnTick();
				}

				var pushAwayOriginPosition = characterBody.corePosition;
				if (pushAwayOrigin) pushAwayOriginPosition = pushAwayOrigin.position;

				var myTeamIndex = GetTeam();
				for (var teamIndex = 0; teamIndex < TeamCatalog.teamDefs.Length; teamIndex++)
				{
					if ((TeamIndex)teamIndex != myTeamIndex)
					{
						foreach (var enemy in TeamComponent.GetTeamMembers((TeamIndex)teamIndex))
						{
							if (enemy.body)
							{
								if (tickTriggered)
									OnTickForEnemyBody(enemy.body);
								
								if (enemy.body.hasEffectiveAuthority)
								{
									var displacementReceiver = enemy.body.GetComponent<IDisplacementReceiver>();
									if (displacementReceiver != null)
									{
										displacementReceiver.AddDisplacement((enemy.body.corePosition - pushAwayOriginPosition).normalized * pushAwayPower * Time.fixedDeltaTime);
									}
								}
							}
						}
					}
				}
			}

			public virtual void OnTick()
            {

            }

			public virtual void OnTickForEnemyBody(CharacterBody body)
            {
				if (NetworkServer.active && debuffDuration > 0)
                {
					body.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_VoidRaidCrabScare, debuffDuration);
				}
            }

            public override void OnExit()
			{
				LoopSoundManager.StopSoundLoopLocal(loopPtr);
				Util.PlaySound("Play_voidRaid_superLaser_end", gameObject);
				PostProcessing.ChallengeModePostProcessing.voidRaidCrabScreamIntensityTarget = 0f;
				if (screamFX) Object.Destroy(screamFX);
				base.OnExit();
            }

            public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class ExitScream : BaseState
		{
			public static float baseDuration = 3.3333f;

			public float duration;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				PlayAnimation("Body", "SuckExit", "Suck.playbackRate", duration);
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class EnterScreamCallForHelp : EnterScream
		{
			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextState(new ScreamCallForHelp());
				}
			}
		}

		public class ScreamCallForHelp : Scream
		{
			public static CharacterSpawnCard helperSpawnCard;

			public override void OnEnter()
			{
				baseDuration = 10f;
				tickFrequency = 3.5f;
				debuffDuration = 4f;
				base.OnEnter();
			}

            public override void OnTick()
            {
                base.OnTick();

				if (DirectorCore.instance)
				{
					var directorSpawnRequest = new DirectorSpawnRequest(helperSpawnCard, new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.Approximate,
						minDistance = 20f,
						maxDistance = 150f,
						spawnOnTarget = transform
					}, RoR2Application.rng);
					directorSpawnRequest.summonerBodyObject = gameObject;
					directorSpawnRequest.onSpawnedServer += (spawnResult) =>
					{
						if (spawnResult.success && spawnResult.spawnedInstance && characterBody)
						{
							var minionInventory = spawnResult.spawnedInstance.GetComponent<Inventory>();
							if (minionInventory)
							{
								minionInventory.CopyEquipmentFrom(characterBody.inventory);
								minionInventory.GiveItem(ChallengeModeContent.Items.ChallengeMode_PermanentImmuneToVoidDeath);
							}
						}
					};
					DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
				}
			}
		}

		public class BaseFireBeamSweep : BaseState
		{
			public static LoopSoundDef loopSoundDef;

			public float baseDuration = 2f;
			public float duration;
			public float windDownDuration = 0.5f;
			public bool woundDown = false;
			public GameObject beamVfxInstance;
			public AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
			public LoopSoundManager.SoundLoopPtr loopPtr;

			public Transform muzzle;
			public Transform head;

			public Ray initialRay;
			public Ray currentRay;
			public Vector3 desiredDirection;
			
			public float horizontalAnglesPerSecond = 0f;
			public float verticalTiltAngles = 0f;
			
			public float aimVelocity;
			public float aimVectorDampTime = 0.1f;
			public float aimVectorMaxSpeed = 40f;

			public float beamTickTimer = 0f;
			public float beamDpsCoefficient = 4f;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat + windDownDuration;
				Util.PlayAttackSpeedSound("Play_voidRaid_superLaser_start", gameObject, attackSpeedStat);
				if (loopSoundDef) loopPtr = LoopSoundManager.PlaySoundLoopLocal(gameObject, loopSoundDef);
				CreateBeamVFXInstance(EntityStates.VoidRaidCrab.SpinBeamAttack.beamVfxPrefab);

				var childLocator = GetModelChildLocator();
				if (childLocator)
				{
					muzzle = childLocator.FindChild("EyeProjectileCenter");
					head = childLocator.FindChild("Head");
				}

				var aimAnimator = GetAimAnimator();
				if (aimAnimator)
				{
					animatorDirectionOverrideRequest = aimAnimator.RequestDirectionOverride(GetAimDirection);
				}

				currentRay = initialRay;
				desiredDirection = currentRay.direction;
				desiredDirection = Quaternion.AngleAxis(verticalTiltAngles, transform.right) * desiredDirection;
			}

			public Vector3 GetAimDirection()
			{
				return currentRay.direction;
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();

				desiredDirection = Quaternion.AngleAxis(horizontalAnglesPerSecond * Time.fixedDeltaTime, Vector3.up) * desiredDirection;

				var targetRotation = Util.QuaternionSafeLookRotation(desiredDirection);
				currentRay.direction = Util.SmoothDampQuaternion(
					Util.QuaternionSafeLookRotation(currentRay.direction),
					targetRotation,
					ref aimVelocity,
					aimVectorDampTime,
					aimVectorMaxSpeed,
					Time.fixedDeltaTime
				) * Vector3.forward;
				
				if (isAuthority)
				{
					if (beamTickTimer <= 0f)
					{
						beamTickTimer += 1f / EntityStates.VoidRaidCrab.SpinBeamAttack.beamTickFrequency;
						FireBeamBulletAuthority();
					}
					beamTickTimer -= Time.fixedDeltaTime;
				}

				if (!woundDown && fixedAge >= (duration - windDownDuration))
                {
					woundDown = true;
					BeginWindDown();
				}

				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public void FireBeamBulletAuthority()
            {
				var beamRay = GetBeamRay();
				new BulletAttack
				{
					muzzleName = EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.muzzleTransformNameInChildLocator,
					origin = beamRay.origin,
					aimVector = beamRay.direction,
					minSpread = 0f,
					maxSpread = 0f,
					maxDistance = 400f,
					hitMask = LayerIndex.CommonMasks.bullet,
					stopperMask = 0,
					bulletCount = 1U,
					radius = EntityStates.VoidRaidCrab.SpinBeamAttack.beamRadius,
					smartCollision = false,
					queryTriggerInteraction = QueryTriggerInteraction.Ignore,
					procCoefficient = 1f,
					owner = gameObject,
					weapon = gameObject,
					damage = beamDpsCoefficient * damageStat / EntityStates.VoidRaidCrab.SpinBeamAttack.beamTickFrequency,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Generic,
					falloffModel = BulletAttack.FalloffModel.None,
					force = 0f,
					hitEffectPrefab = EntityStates.VoidRaidCrab.SpinBeamAttack.beamImpactEffectPrefab,
					tracerEffectPrefab = null,
					isCrit = false,
					HitEffectNormal = false
				}.Fire();
			}

			public void CreateBeamVFXInstance(GameObject beamVfxPrefab)
			{
				if (beamVfxInstance == null)
				{
					beamVfxInstance = Object.Instantiate(beamVfxPrefab);
					beamVfxInstance.transform.SetParent(FindModelChild(EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.headTransformNameInChildLocator), true);
					UpdateBeamTransforms();
					RoR2Application.onLateUpdate += UpdateBeamTransformsInLateUpdate;
				}
			}

			public void DestroyBeamVFXInstance()
			{
				if (beamVfxInstance != null)
				{
					RoR2Application.onLateUpdate -= UpdateBeamTransformsInLateUpdate;
					VfxKillBehavior.KillVfxObject(beamVfxInstance);
					beamVfxInstance = null;
				}
			}

			public void UpdateBeamTransformsInLateUpdate()
			{
				try
				{
					UpdateBeamTransforms();
				}
				catch { }
			}

			public Ray GetBeamRay()
			{
				if (muzzle && head)
				{
					return new Ray(muzzle.position, currentRay.direction);
				}
				return GetAimRay();
			}

			private void UpdateBeamTransforms()
			{
				var beamRay = GetBeamRay();
				beamVfxInstance.transform.SetPositionAndRotation(beamRay.origin, Quaternion.LookRotation(beamRay.direction));
			}

			public void BeginWindDown()
            {
				if (animatorDirectionOverrideRequest != null) animatorDirectionOverrideRequest.Dispose();
				LoopSoundManager.StopSoundLoopLocal(loopPtr);
				DestroyBeamVFXInstance();
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class EnterBeamSweepTurning : BaseState
		{
			public static float baseDuration = 2f;

			public float duration;
			public GameObject beamVfxInstance;
			public Transform muzzle;
			public Transform head;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				Util.PlayAttackSpeedSound("Play_voidRaid_superLaser_chargeUp", gameObject, attackSpeedStat);

				var childLocator = GetModelChildLocator();
				if (childLocator)
				{
					muzzle = childLocator.FindChild("EyeProjectileCenter");
					head = childLocator.FindChild("Head");
				}
				CreateBeamVFXInstance(EntityStates.VoidRaidCrab.SpinBeamWindUp.warningLaserPrefab);
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextState(new FireBeamSweepTurning());
				}
			}

			public void CreateBeamVFXInstance(GameObject beamVfxPrefab)
			{
				if (beamVfxInstance == null)
				{
					beamVfxInstance = Object.Instantiate(beamVfxPrefab);
					beamVfxInstance.transform.SetParent(FindModelChild(EntityStates.VoidRaidCrab.BaseSpinBeamAttackState.headTransformNameInChildLocator), true);
					UpdateBeamTransforms();
					RoR2Application.onLateUpdate += UpdateBeamTransformsInLateUpdate;
				}
			}

			public void DestroyBeamVFXInstance()
			{
				if (beamVfxInstance != null)
				{
					RoR2Application.onLateUpdate -= UpdateBeamTransformsInLateUpdate;
					VfxKillBehavior.KillVfxObject(beamVfxInstance);
					beamVfxInstance = null;
				}
			}

			public void UpdateBeamTransformsInLateUpdate()
			{
				try
				{
					UpdateBeamTransforms();
				}
				catch { }
			}

			public Ray GetBeamRay()
            {
				if (muzzle && head)
				{
					return new Ray(muzzle.position, head.forward);
				}
				return GetAimRay();
			}

			private void UpdateBeamTransforms()
			{
				var beamRay = GetBeamRay();
				beamVfxInstance.transform.SetPositionAndRotation(beamRay.origin, Quaternion.LookRotation(beamRay.direction));
			}

			public override void OnExit()
			{
				DestroyBeamVFXInstance();
				base.OnExit();
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}

		public class FireBeamSweepTurning : BaseFireBeamSweep
        {
			public int beamCount = 0;
			public int totalBeamCount = 3;

            public override void OnEnter()
            {
				if (beamCount == 0)
				{
					horizontalAnglesPerSecond = 17f;
					if (Util.CheckRoll(50f)) horizontalAnglesPerSecond = -17f;
					initialRay = GetAimRay();
					var twoDimensional = new Vector2(initialRay.direction.x, initialRay.direction.z).normalized;
					initialRay.direction = new Vector3(twoDimensional.x, 0f, twoDimensional.y);
					verticalTiltAngles = 27.5f;
				}
				else
				{
					initialRay.direction = new Vector3(initialRay.direction.x, currentRay.direction.y, initialRay.direction.z);
					horizontalAnglesPerSecond = -horizontalAnglesPerSecond;
					verticalTiltAngles = 0f;
				}
				duration = 1.3f;
                base.OnEnter();
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					var newBeamCount = beamCount + 1;
					if (newBeamCount < totalBeamCount)
                    {
						outer.SetNextState(new FireBeamSweepTurning
						{
							beamCount = newBeamCount,
							horizontalAnglesPerSecond = horizontalAnglesPerSecond,
							initialRay = initialRay,
							currentRay = currentRay
						});
                    }
                    else
                    {
						outer.SetNextState(new ExitBeamSweep());
                    }
				}
			}
		}

		public class ExitBeamSweep : BaseState
		{
			public static float baseDuration = 3f;

			public float duration;

			public override void OnEnter()
			{
				base.OnEnter();
				duration = baseDuration / attackSpeedStat;
				Util.PlayAttackSpeedSound("Play_voidRaid_superLaser_end", gameObject, attackSpeedStat);
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public override InterruptPriority GetMinimumInterruptPriority()
			{
				return InterruptPriority.PrioritySkill;
			}
		}
	}
}
