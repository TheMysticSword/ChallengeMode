using MysticsRisky2Utils;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Special
{
    public class VoidRaidCrabEX : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_VOIDRAIDCRABEX_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_VOIDRAIDCRABEX_DESC";
        public override bool isAdditional => true;

        public BodyIndex bodyIndexPhase1;
        public BodyIndex bodyIndexPhase2;
        public BodyIndex bodyIndexPhase3;

        public float baseHealthMultiplier = 0.575f;

        public bool voidOutbreakActive = false;
        public float voidOutbreakTimer = 0f;
        public float voidOutbreakInterval = 120f;
        public List<CharacterSpawnCard> voidOutbreakSpawnCards = new List<CharacterSpawnCard>();
        public GameObject voidOutbreakSpawnEffect;
        public float voidOutbreakSpawnEffectScale = 25f;
        
        public bool rockFallActive = false;
        public float rockFallTimer = 0f;
        public float rockFallInterval = 2f;
        public float rockFallMinRange = 100f;
        public float rockFallMaxRange = 200f;
        public float rockFallHeightOffsetMin = 50f;
        public float rockFallHeightOffsetMax = 70f;
        public int rockFallCount = 1;
        public GameObject rockFallProjectile;
        public float rockFallSpeedOverride = 40f;
        public float rockFallDamageCoefficient = 2f;

        public Vector3 activeDonutCenter = Vector3.zero;
        public VoidRaidGauntletController.DonutInfo _currentDonut;
        public VoidRaidGauntletController.DonutInfo _previousDonut;

        public List<GameObject> arenaEffectsInstances = new List<GameObject>();

        public GameObject currentEnemyCrab;
        public CharacterBody currentEnemyCrabBody;

        public override void OnEnable()
        {
            base.OnEnable();

            voidOutbreakActive = false;
            voidOutbreakTimer = 0f;

            rockFallActive = false;
            rockFallTimer = 0f;

            bodyIndexPhase1 = BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyPhase1");
            bodyIndexPhase2 = BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyPhase2");
            bodyIndexPhase3 = BodyCatalog.FindBodyIndex("MiniVoidRaidCrabBodyPhase3");

            if (voidOutbreakSpawnCards.Count <= 0)
            {
                voidOutbreakSpawnCards.Add(Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Titan/cscTitanBlackBeach.asset").WaitForCompletion());
                voidOutbreakSpawnCards.Add(Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Vagrant/cscVagrant.asset").WaitForCompletion());
                voidOutbreakSpawnCards.Add(Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/ClayBoss/cscClayBoss.asset").WaitForCompletion());
            }

            if (!voidOutbreakSpawnEffect)
                voidOutbreakSpawnEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab").WaitForCompletion();

            if (!rockFallProjectile)
                rockFallProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Grandparent/GrandparentBoulder.prefab").WaitForCompletion();

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            On.EntityStates.VoidRaidCrab.SpawnState.FixedUpdate += SpawnState_FixedUpdate;
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            if (self.bodyIndex == bodyIndexPhase1 || self.bodyIndex == bodyIndexPhase2 || self.bodyIndex == bodyIndexPhase3)
            {
                self.baseMaxHealth *= baseHealthMultiplier;
                self.levelMaxHealth *= baseHealthMultiplier;

                if (self.modelLocator && self.modelLocator.modelTransform)
                {
                    var characterModel = self.modelLocator.modelTransform.GetComponent<CharacterModel>();
                    if (characterModel)
                    {
                        for (var i = 0; i < characterModel.baseRendererInfos.Length; i++)
                        {
                            var material = characterModel.baseRendererInfos[i].defaultMaterial;
                            if (material.shader.name == "Standard" || material.shader == HopooShaderToMaterial.Standard.shader)
                            {
                                characterModel.baseRendererInfos[i].ignoreOverlays = false;
                            }
                        }
                    }
                }
                EXBossAssets.fightEffectsActive = true;
            }
            orig(self);
        }

        public static void SetUpBodySkillLocator(CharacterBody body, SkillDef primaryDef = null, SkillDef secondaryDef = null, SkillDef utilityDef = null, SkillDef specialDef = null)
        {
            if (body)
            {
                body.gameObject.SetActive(false); // to prevent calling GenericSkill.Awake too early

                GenericSkill primary = null;
                if (primaryDef)
                {
                    primary = body.gameObject.AddComponent<GenericSkill>();
                    primary._skillFamily = VoidRaidCrabEXAssets.skillFamilyPrimary;
                    body.skillLocator.primary = primary;
                }

                GenericSkill secondary = null;
                if (secondaryDef)
                {
                    secondary = body.gameObject.AddComponent<GenericSkill>();
                    secondary._skillFamily = VoidRaidCrabEXAssets.skillFamilySecondary;
                    body.skillLocator.secondary = secondary;
                }

                GenericSkill utility = null;
                if (utilityDef)
                {
                    utility = body.gameObject.AddComponent<GenericSkill>();
                    utility._skillFamily = VoidRaidCrabEXAssets.skillFamilyUtility;
                    body.skillLocator.utility = utility;
                }

                GenericSkill special = null;
                if (specialDef)
                {
                    special = body.gameObject.AddComponent<GenericSkill>();
                    special._skillFamily = VoidRaidCrabEXAssets.skillFamilySpecial;
                    body.skillLocator.special = special;
                }

                body.gameObject.SetActive(true);

                void ApplySkill(GenericSkill skillHolder, SkillDef skillDef)
                {
                    if (skillHolder && skillDef)
                    {
                        skillHolder.SetBaseSkill(skillDef);
                        skillHolder.skillName = skillDef.skillName;
                    }
                }
                ApplySkill(primary, primaryDef);
                ApplySkill(secondary, secondaryDef);
                ApplySkill(utility, utilityDef);
                ApplySkill(special, specialDef);
            }
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (body.bodyIndex == bodyIndexPhase1 || body.bodyIndex == bodyIndexPhase2 || body.bodyIndex == bodyIndexPhase3)
            {
                if (NetworkServer.active && !body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_EXBoss))
                    body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_EXBoss);

                var skills = body.GetComponentsInChildren<GenericSkill>().ToList();
                foreach (var skill in skills)
                    Object.Destroy(skill);
                body.skillLocator.primary = null;
                body.skillLocator.secondary = null;
                body.skillLocator.utility = null;
                body.skillLocator.special = null;

                if (body.master)
                {
                    foreach (var ai in body.master.aiComponents)
                    {
                        foreach (var skillDriver in ai.skillDrivers.ToList())
                        {
                            Object.Destroy(skillDriver);
                        }
                        ai.skillDrivers = new AISkillDriver[0];
                    }
                }

                activeDonutCenter = VoidRaidCrabEXAssets.GetActiveDonutPosition(body.corePosition);
                if (VoidRaidGauntletController.instance.currentDonut != _currentDonut || VoidRaidGauntletController.instance.previousDonut != _previousDonut)
                {
                    _currentDonut = VoidRaidGauntletController.instance.currentDonut;
                    _previousDonut = VoidRaidGauntletController.instance.previousDonut;
                    arenaEffectsInstances.Add(Object.Instantiate(VoidRaidCrabEXAssets.arenaEffectsPrefab, activeDonutCenter, Quaternion.identity));
                }

                if (body.teamComponent.teamIndex == TeamIndex.Void)
                {
                    currentEnemyCrab = body.gameObject;
                    currentEnemyCrabBody = body;
                }
            }

            if (body.bodyIndex == bodyIndexPhase1)
            {
                SetUpBodySkillLocator(
                    body,
                    VoidRaidCrabEXAssets.skillDisarmingOrbMinigun,
                    VoidRaidCrabEXAssets.skillDirectLasers,
                    VoidRaidCrabEXAssets.skillKnockUpEveryone,
                    VoidRaidCrabEXAssets.skillScream
                );

                if (body.master && body.master.aiComponents.Length > 0)
                {
                    var fireDisarmingOrbs = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireDisarmingOrbs.customName = "FireDisarmingOrbs";
                    fireDisarmingOrbs.skillSlot = SkillSlot.Primary;
                    fireDisarmingOrbs.requireSkillReady = true;
                    fireDisarmingOrbs.minDistance = 0f;
                    fireDisarmingOrbs.maxDistance = 500f;
                    fireDisarmingOrbs.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireDisarmingOrbs.maxUserHealthFraction = Mathf.Infinity;
                    fireDisarmingOrbs.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireDisarmingOrbs.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    fireDisarmingOrbs.moveInputScale = 0.5f;
                    fireDisarmingOrbs.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireDisarmingOrbs.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireDisarmingOrbs.driverUpdateTimerOverride = 0.1f;

                    var knockUpEveryone = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    knockUpEveryone.customName = "KnockUpEveryone";
                    knockUpEveryone.skillSlot = SkillSlot.Utility;
                    knockUpEveryone.requireSkillReady = true;
                    knockUpEveryone.minDistance = 0f;
                    knockUpEveryone.maxDistance = 500f;
                    knockUpEveryone.minUserHealthFraction = Mathf.NegativeInfinity;
                    knockUpEveryone.maxUserHealthFraction = 0.6f;
                    knockUpEveryone.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    knockUpEveryone.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    knockUpEveryone.moveInputScale = 0.5f;
                    knockUpEveryone.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    knockUpEveryone.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    knockUpEveryone.driverUpdateTimerOverride = 1f;
                    knockUpEveryone.noRepeat = true;

                    var scream = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    scream.customName = "Scream";
                    scream.skillSlot = SkillSlot.Special;
                    scream.requireSkillReady = true;
                    scream.minDistance = 0f;
                    scream.maxDistance = 500f;
                    scream.minUserHealthFraction = Mathf.NegativeInfinity;
                    scream.maxUserHealthFraction = 0.5f;
                    scream.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    scream.movementType = AISkillDriver.MovementType.Stop;
                    scream.moveInputScale = 1f;
                    scream.aimType = AISkillDriver.AimType.MoveDirection;
                    scream.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    scream.driverUpdateTimerOverride = 1f;
                    scream.noRepeat = true;

                    var fireDirectLasers = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireDirectLasers.customName = "FireDirectLasers";
                    fireDirectLasers.skillSlot = SkillSlot.Secondary;
                    fireDirectLasers.requireSkillReady = true;
                    fireDirectLasers.minDistance = 0f;
                    fireDirectLasers.maxDistance = 400f;
                    fireDirectLasers.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireDirectLasers.maxUserHealthFraction = 0.9f;
                    fireDirectLasers.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireDirectLasers.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                    fireDirectLasers.moveInputScale = 0.5f;
                    fireDirectLasers.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireDirectLasers.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireDirectLasers.driverUpdateTimerOverride = 1f;

                    var backUp = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    backUp.customName = "BackUp";
                    backUp.minDistance = 0f;
                    backUp.maxDistance = 100f;
                    backUp.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    backUp.movementType = AISkillDriver.MovementType.Stop;
                    backUp.moveInputScale = 1f;
                    backUp.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    backUp.driverUpdateTimerOverride = 1f;
                    backUp.noRepeat = true;
                    
                    var strafeTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    strafeTarget.customName = "StrafeTarget";
                    strafeTarget.minDistance = 100f;
                    strafeTarget.maxDistance = 200f;
                    strafeTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    strafeTarget.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                    strafeTarget.moveInputScale = 0.5f;
                    strafeTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    strafeTarget.driverUpdateTimerOverride = 2f;

                    var chaseTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    chaseTarget.customName = "ChaseTarget";
                    chaseTarget.minDistance = 0f;
                    chaseTarget.maxDistance = Mathf.Infinity;
                    chaseTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    chaseTarget.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    chaseTarget.moveInputScale = 1f;
                    chaseTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    chaseTarget.driverUpdateTimerOverride = 2f;

                    body.master.aiComponents[0].skillDrivers = new AISkillDriver[]
                    {
                        fireDisarmingOrbs,
                        knockUpEveryone,
                        scream,
                        fireDirectLasers,
                        backUp,
                        strafeTarget,
                        chaseTarget
                    };
                }
            }

            if (body.bodyIndex == bodyIndexPhase2)
            {
                SetUpBodySkillLocator(
                    body,
                    VoidRaidCrabEXAssets.skillAheadLaserWithPool,
                    VoidRaidCrabEXAssets.skillReaverBombPattern,
                    VoidRaidCrabEXAssets.skillVacuum,
                    VoidRaidCrabEXAssets.skillScreamCallForHelp
                );

                if (body.master && body.master.aiComponents.Length > 0)
                {
                    var screamCallForHelp = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    screamCallForHelp.customName = "ScreamCallForHelp";
                    screamCallForHelp.skillSlot = SkillSlot.Special;
                    screamCallForHelp.requireSkillReady = true;
                    screamCallForHelp.minDistance = 0f;
                    screamCallForHelp.maxDistance = 500f;
                    screamCallForHelp.minUserHealthFraction = Mathf.NegativeInfinity;
                    screamCallForHelp.maxUserHealthFraction = 0.6f;
                    screamCallForHelp.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    screamCallForHelp.movementType = AISkillDriver.MovementType.Stop;
                    screamCallForHelp.moveInputScale = 1f;
                    screamCallForHelp.aimType = AISkillDriver.AimType.MoveDirection;
                    screamCallForHelp.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    screamCallForHelp.driverUpdateTimerOverride = 1f;
                    screamCallForHelp.noRepeat = true;

                    var fireReaverBombPattern = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireReaverBombPattern.customName = "FireReaverBombPattern";
                    fireReaverBombPattern.skillSlot = SkillSlot.Secondary;
                    fireReaverBombPattern.requireSkillReady = true;
                    fireReaverBombPattern.minDistance = 0f;
                    fireReaverBombPattern.maxDistance = 500f;
                    fireReaverBombPattern.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireReaverBombPattern.maxUserHealthFraction = 0.95f;
                    fireReaverBombPattern.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireReaverBombPattern.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                    fireReaverBombPattern.moveInputScale = 0.5f;
                    fireReaverBombPattern.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireReaverBombPattern.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireReaverBombPattern.driverUpdateTimerOverride = 0.1f;

                    var fireAheadLasers = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireAheadLasers.customName = "FireAheadLasers";
                    fireAheadLasers.skillSlot = SkillSlot.Primary;
                    fireAheadLasers.requireSkillReady = true;
                    fireAheadLasers.minDistance = 0f;
                    fireAheadLasers.maxDistance = 200f;
                    fireAheadLasers.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireAheadLasers.maxUserHealthFraction = Mathf.Infinity;
                    fireAheadLasers.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireAheadLasers.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    fireAheadLasers.moveInputScale = 0.5f;
                    fireAheadLasers.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireAheadLasers.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireAheadLasers.driverUpdateTimerOverride = 0.1f;

                    var vacuum = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    vacuum.customName = "Vacuum";
                    vacuum.skillSlot = SkillSlot.Utility;
                    vacuum.requireSkillReady = true;
                    vacuum.minDistance = 0f;
                    vacuum.maxDistance = 500f;
                    vacuum.minUserHealthFraction = Mathf.NegativeInfinity;
                    vacuum.maxUserHealthFraction = 0.38f;
                    vacuum.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    vacuum.movementType = AISkillDriver.MovementType.Stop;
                    vacuum.moveInputScale = 1f;
                    vacuum.aimType = AISkillDriver.AimType.MoveDirection;
                    vacuum.buttonPressType = AISkillDriver.ButtonPressType.TapContinuous;
                    vacuum.driverUpdateTimerOverride = 17f;
                    vacuum.noRepeat = true;

                    var fleeTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fleeTarget.customName = "FleeTarget";
                    fleeTarget.minDistance = 0f;
                    fleeTarget.maxDistance = 100f;
                    fleeTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fleeTarget.movementType = AISkillDriver.MovementType.Stop;
                    fleeTarget.moveInputScale = 1f;
                    fleeTarget.aimType = AISkillDriver.AimType.AtMoveTarget;
                    fleeTarget.driverUpdateTimerOverride = 1f;
                    fleeTarget.noRepeat = true;
                    
                    var strafeTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    strafeTarget.customName = "StrafeTarget";
                    strafeTarget.minDistance = 100f;
                    strafeTarget.maxDistance = 200f;
                    strafeTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    strafeTarget.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                    strafeTarget.moveInputScale = 0.5f;
                    strafeTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    strafeTarget.driverUpdateTimerOverride = 1f;

                    var chaseTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    chaseTarget.customName = "ChaseTarget";
                    chaseTarget.minDistance = 200f;
                    chaseTarget.maxDistance = Mathf.Infinity;
                    chaseTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    chaseTarget.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    chaseTarget.moveInputScale = 1f;
                    chaseTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    chaseTarget.driverUpdateTimerOverride = 2f;
                    chaseTarget.ignoreNodeGraph = false;

                    body.master.aiComponents[0].skillDrivers = new AISkillDriver[]
                    {
                        screamCallForHelp,
                        fireReaverBombPattern,
                        fireAheadLasers,
                        vacuum,
                        fleeTarget,
                        strafeTarget,
                        chaseTarget,
                    };
                }
            }

            if (body.bodyIndex == bodyIndexPhase3)
            {
                voidOutbreakActive = true;
                rockFallActive = true;

                SetUpBodySkillLocator(
                    body,
                    VoidRaidCrabEXAssets.skillDisarmingOrbMinigun,
                    VoidRaidCrabEXAssets.skillDevastatorBombMortar,
                    VoidRaidCrabEXAssets.skillReaverBombSnake,
                    VoidRaidCrabEXAssets.skillBeamSweep
                );

                if (body.master && body.master.aiComponents.Length > 0)
                {
                    var fireBeamSweep = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireBeamSweep.customName = "FireBeamSweep";
                    fireBeamSweep.skillSlot = SkillSlot.Special;
                    fireBeamSweep.requireSkillReady = true;
                    fireBeamSweep.minDistance = 0f;
                    fireBeamSweep.maxDistance = 200f;
                    fireBeamSweep.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireBeamSweep.maxUserHealthFraction = 0.6f;
                    fireBeamSweep.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireBeamSweep.movementType = AISkillDriver.MovementType.Stop;
                    fireBeamSweep.moveInputScale = 1f;
                    fireBeamSweep.aimType = AISkillDriver.AimType.MoveDirection;
                    fireBeamSweep.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireBeamSweep.driverUpdateTimerOverride = 1f;

                    var fireReaverBombSnake = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireReaverBombSnake.customName = "FireReaverBombSnake";
                    fireReaverBombSnake.skillSlot = SkillSlot.Utility;
                    fireReaverBombSnake.requireSkillReady = true;
                    fireReaverBombSnake.minDistance = 0f;
                    fireReaverBombSnake.maxDistance = 500f;
                    fireReaverBombSnake.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireReaverBombSnake.maxUserHealthFraction = 0.6f;
                    fireReaverBombSnake.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireReaverBombSnake.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    fireReaverBombSnake.moveInputScale = 0.5f;
                    fireReaverBombSnake.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireReaverBombSnake.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireReaverBombSnake.driverUpdateTimerOverride = 1f;

                    var fireDevastatorMortar = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireDevastatorMortar.customName = "FireDevastatorMortar";
                    fireDevastatorMortar.skillSlot = SkillSlot.Secondary;
                    fireDevastatorMortar.requireSkillReady = true;
                    fireDevastatorMortar.minDistance = 0f;
                    fireDevastatorMortar.maxDistance = 300f;
                    fireDevastatorMortar.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireDevastatorMortar.maxUserHealthFraction = Mathf.Infinity;
                    fireDevastatorMortar.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireDevastatorMortar.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    fireDevastatorMortar.moveInputScale = 0.5f;
                    fireDevastatorMortar.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireDevastatorMortar.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireDevastatorMortar.driverUpdateTimerOverride = 1f;

                    var fireDisarmingOrbs = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fireDisarmingOrbs.customName = "FireDisarmingOrbs";
                    fireDisarmingOrbs.skillSlot = SkillSlot.Primary;
                    fireDisarmingOrbs.requireSkillReady = true;
                    fireDisarmingOrbs.minDistance = 0f;
                    fireDisarmingOrbs.maxDistance = 300f;
                    fireDisarmingOrbs.minUserHealthFraction = Mathf.NegativeInfinity;
                    fireDisarmingOrbs.maxUserHealthFraction = Mathf.Infinity;
                    fireDisarmingOrbs.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fireDisarmingOrbs.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    fireDisarmingOrbs.moveInputScale = 0.5f;
                    fireDisarmingOrbs.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fireDisarmingOrbs.buttonPressType = AISkillDriver.ButtonPressType.Hold;
                    fireDisarmingOrbs.driverUpdateTimerOverride = 0.1f;

                    var fleeTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    fleeTarget.customName = "Stop";
                    fleeTarget.minDistance = 0f;
                    fleeTarget.maxDistance = 100f;
                    fleeTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    fleeTarget.movementType = AISkillDriver.MovementType.Stop;
                    fleeTarget.moveInputScale = 1f;
                    fleeTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    fleeTarget.driverUpdateTimerOverride = 1f;
                    fleeTarget.ignoreNodeGraph = false;
                    fleeTarget.noRepeat = true;
                    
                    var strafeTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    strafeTarget.customName = "StrafeTarget";
                    strafeTarget.minDistance = 100f;
                    strafeTarget.maxDistance = 200f;
                    strafeTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    strafeTarget.movementType = AISkillDriver.MovementType.StrafeMovetarget;
                    strafeTarget.moveInputScale = 0.5f;
                    strafeTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    strafeTarget.driverUpdateTimerOverride = 1f;

                    var chaseTarget = body.master.aiComponents[0].gameObject.AddComponent<AISkillDriver>();
                    chaseTarget.customName = "ChaseTarget";
                    chaseTarget.minDistance = 200f;
                    chaseTarget.maxDistance = Mathf.Infinity;
                    chaseTarget.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
                    chaseTarget.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
                    chaseTarget.moveInputScale = 1f;
                    chaseTarget.aimType = AISkillDriver.AimType.AtCurrentEnemy;
                    chaseTarget.driverUpdateTimerOverride = 1f;
                    chaseTarget.ignoreNodeGraph = false;

                    body.master.aiComponents[0].skillDrivers = new AISkillDriver[]
                    {
                        fireBeamSweep,
                        fireReaverBombSnake,
                        fireDevastatorMortar,
                        fireDisarmingOrbs,
                        fleeTarget,
                        strafeTarget,
                        chaseTarget,
                    };
                }
            }
        }

        private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            if (self.bodyIndex == bodyIndexPhase1 || self.bodyIndex == bodyIndexPhase2 || self.bodyIndex == bodyIndexPhase3)
            {
                self.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_EXBoss);

                if (self.bodyIndex == bodyIndexPhase3 && self.teamComponent.teamIndex == TeamIndex.Void)
                {
                    var anyoneAlive = false;
                    foreach (var voidAlly in TeamComponent.GetTeamMembers(TeamIndex.Void))
                    {
                        if (voidAlly.body && voidAlly.body.bodyIndex == bodyIndexPhase3 && voidAlly.body.master && !voidAlly.body.master.IsDeadAndOutOfLivesServer())
                        {
                            anyoneAlive = true;
                            break;
                        }
                    }

                    if (!anyoneAlive)
                    {
                        DestroyAllWorldEffects();

                        voidOutbreakActive = false;
                        rockFallActive = false;

                        foreach (var enemy in TeamComponent.GetTeamMembers(TeamIndex.Monster))
                        {
                            if (enemy.body && enemy.body.healthComponent && enemy.body.healthComponent.alive)
                            {
                                enemy.body.healthComponent.Suicide(null, null, DamageType.VoidDeath);
                            }
                        }
                    }
                }
            }
            orig(self);
        }

        private void SpawnState_FixedUpdate(On.EntityStates.VoidRaidCrab.SpawnState.orig_FixedUpdate orig, EntityStates.VoidRaidCrab.SpawnState self)
        {
            orig(self);
            if (self.fixedAge >= self.duration && NetworkServer.active && self.GetTeam() == TeamIndex.Void)
            {
                var body = self.characterBody;
                if (body)
                {
                    foreach (var combatDirector in CombatDirector.instancesList)
                    {
                        if (combatDirector.enabled)
                        {
                            if (body.bodyIndex == bodyIndexPhase2)
                            {
                                MultiplyCombatDirectorCreditGain(combatDirector, 1.5f);
                            }
                            if (body.bodyIndex == bodyIndexPhase3)
                            {
                                MultiplyCombatDirectorCreditGain(combatDirector, 2f);
                            }
                        }
                    }
                }
            }
        }

        public void ProcessVoidOutbreak()
        {
            if (!voidOutbreakActive || !currentEnemyCrab) return;

            voidOutbreakTimer -= Time.fixedDeltaTime;
            if (voidOutbreakTimer <= 0)
            {
                voidOutbreakTimer += voidOutbreakInterval;

                if (DirectorCore.instance && voidOutbreakSpawnCards.Count > 0)
                {
                    var spawnCard = RoR2Application.rng.NextElementUniform(voidOutbreakSpawnCards);
                    if (!spawnCard) return;

                    var nodeGraph = SceneInfo.instance.GetNodeGraph(spawnCard.nodeGraphType);
                    var spawnNodes = nodeGraph.FindNodesInRangeWithFlagConditions(
                        activeDonutCenter,
                        100f,
                        200f,
                        (HullMask)(1 << (int)spawnCard.hullSize),
                        spawnCard.requiredFlags,
                        spawnCard.forbiddenFlags,
                        false
                    );
                    var spawnPosition = Vector3.zero;

                    while (spawnNodes.Count > 0)
                    {
                        var i = RoR2Application.rng.RangeInt(0, spawnNodes.Count);
                        var nodeIndex = spawnNodes[i];
                        if (nodeGraph.GetNodePosition(nodeIndex, out var nodePosition) && DirectorCore.instance.CheckPositionFree(nodeGraph, nodeIndex, spawnCard))
                        {
                            spawnPosition = nodePosition;
                            break;
                        }
                    }
                    
                    var directorSpawnRequest = new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Direct,
                        position = spawnPosition
                    }, RoR2Application.rng);
                    directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
                    directorSpawnRequest.onSpawnedServer += (spawnResult) =>
                    {
                        if (spawnResult.success && spawnResult.spawnedInstance)
                        {
                            if (voidOutbreakSpawnEffect)
                                EffectManager.SpawnEffect(voidOutbreakSpawnEffect, new EffectData
                                {
                                    origin = spawnPosition,
                                    scale = voidOutbreakSpawnEffectScale
                                }, true);

                            var ai = spawnResult.spawnedInstance.GetComponent<BaseAI>();
                            if (ai)
                            {
                                void onBodyDiscovered(CharacterBody myBody)
                                {
                                    ai.currentEnemy.gameObject = currentEnemyCrab;
                                    ai.onBodyDiscovered -= onBodyDiscovered;
                                }
                                ai.onBodyDiscovered += onBodyDiscovered;
                            }
                        }
                    };
                    DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
                }
            }
        }

        public void ProcessRockFall()
        {
            if (!rockFallActive) return;

            rockFallTimer -= Time.fixedDeltaTime;
            if (rockFallTimer <= 0)
            {
                rockFallTimer += rockFallInterval;

                if (currentEnemyCrab)
                {
                    for (var i = 0; i < rockFallCount; i++)
                    {
                        var lineForward = Quaternion.AngleAxis(RoR2Application.rng.RangeFloat(0f, 360f), Vector3.up) * Vector3.forward;
                        var rockPosition = activeDonutCenter;
                        rockPosition += lineForward * RoR2Application.rng.RangeFloat(rockFallMinRange, rockFallMaxRange);
                        rockPosition += Vector3.up * RoR2Application.rng.RangeFloat(rockFallHeightOffsetMin, rockFallHeightOffsetMax);

                        var fireProjectileInfo = new FireProjectileInfo
                        {
                            projectilePrefab = rockFallProjectile,
                            position = rockPosition,
                            rotation = Util.QuaternionSafeLookRotation(Vector3.down),
                            owner = currentEnemyCrab,
                            damage = currentEnemyCrabBody.damage,
                            crit = currentEnemyCrabBody.RollCrit(),
                            speedOverride = rockFallSpeedOverride
                        };
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                }
            }
        }

        private void RoR2Application_onFixedUpdate()
        {
            if (NetworkServer.active)
            {
                ProcessVoidOutbreak();
                ProcessRockFall();
            }
        }

        public static void MakeCombatDirectorTargetCrab(CombatDirector combatDirector, GameObject crabObject)
        {
            combatDirector.targetPlayers = false;
            combatDirector.currentSpawnTarget = crabObject;
        }

        public static void MultiplyCombatDirectorCreditGain(CombatDirector combatDirector, float multiplier)
        {
            foreach (var moneyWave in combatDirector.moneyWaves)
            {
                moneyWave.multiplier *= multiplier;
            }
        }

        public void DestroyAllWorldEffects()
        {
            foreach (var arenaEffectsInstance in arenaEffectsInstances)
                if (arenaEffectsInstance)
                    Object.Destroy(arenaEffectsInstance);
            arenaEffectsInstances.Clear();
            EXBossAssets.fightEffectsActive = false;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            CharacterBody.onBodyStartGlobal -= CharacterBody_onBodyStartGlobal;
            On.RoR2.CharacterBody.OnDeathStart -= CharacterBody_OnDeathStart;
            On.EntityStates.VoidRaidCrab.SpawnState.FixedUpdate -= SpawnState_FixedUpdate;

            DestroyAllWorldEffects();
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageNameMatches("voidraid") && Util.CheckRoll(ChallengeModeConfig.specialModifierChance);
        }
    }
}