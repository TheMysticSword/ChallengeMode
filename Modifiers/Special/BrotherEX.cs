using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Special
{
    public class BrotherEX : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BROTHEREX_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BROTHEREX_DESC";
        public override bool isAdditional => true;

        public BodyIndex brotherBodyIndex;
        public BodyIndex brotherHurtBodyIndex;
        public MusicTrackDef replacementSong;

        private float oldWaveProjectileArc = 0f;

        public GameObject arenaEffects;
        public CharacterMaster brotherDummyAttackerMaster;

        public override void OnEnable()
        {
            base.OnEnable();

            brotherBodyIndex = BodyCatalog.FindBodyIndex("BrotherBody");
            brotherHurtBodyIndex = BodyCatalog.FindBodyIndex("BrotherHurtBody");
            replacementSong = MusicTrackCatalog.FindMusicTrackDef("muSong23");

            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
            On.RoR2.MusicController.PickCurrentTrack += MusicController_PickCurrentTrack;
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
            On.EntityStates.Missions.BrotherEncounter.Phase1.PreEncounterBegin += Phase1_PreEncounterBegin;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter += EncounterFinished_OnEnter;

            // hammer slam knocks up, and sends a 360 shockwave in phase 3
            On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlam_FixedUpdate;
            oldWaveProjectileArc = EntityStates.BrotherMonster.WeaponSlam.waveProjectileArc;
            EntityStates.BrotherMonster.WeaponSlam.waveProjectileArc = 360f;
            EntityStates.BrotherMonster.WeaponSlam.waveProjectileCount += 6;
            // sprint bash destroys an item
            On.EntityStates.BrotherMonster.SprintBash.OnEnter += SprintBash_OnEnter;
            // sky leap is longer and homes onto the player
            On.EntityStates.BrotherMonster.HoldSkyLeap.OnEnter += HoldSkyLeap_OnEnter;
            On.EntityStates.BrotherMonster.HoldSkyLeap.FixedUpdate += HoldSkyLeap_FixedUpdate;
            On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
            // dash is faster
            EntityStates.BrotherMonster.BaseSlideState.duration *= 0.5f;
            // big spinny sends shockwaves and knocks up
            On.EntityStates.BrotherMonster.UltChannelState.FireWave += UltChannelState_FireWave;
            // fist slam knocks up
            On.EntityStates.BrotherMonster.FistSlam.FixedUpdate += FistSlam_FixedUpdate;
            // shards end with a tracking bomb
            On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShards_OnEnter;
            // phase 2 summons the moderate spinny
            On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += Phase2_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.FixedUpdate += Phase2_FixedUpdate;
            // meteorites fall in phases 3 & 4
            On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.FixedUpdate += BrotherEncounterPhaseBaseState_FixedUpdate;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody body)
        {
            if (NetworkServer.active &&
                (body.bodyIndex == brotherBodyIndex || body.bodyIndex == brotherHurtBodyIndex) &&
                !body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_EXBoss))
                body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_EXBoss);
        }

        private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            if (self.bodyIndex == brotherBodyIndex || self.bodyIndex == brotherHurtBodyIndex)
                self.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_EXBoss);
            orig(self);
        }

        private void MusicController_PickCurrentTrack(On.RoR2.MusicController.orig_PickCurrentTrack orig, MusicController self, ref MusicTrackDef newTrack)
        {
            orig(self, ref newTrack);
            if (newTrack.cachedName == "muSong25") newTrack = replacementSong;
        }

        private string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, Language self, string token)
        {
            if (token == "BROTHER_DIALOGUE_FORMAT") return self.GetLocalizedFormattedStringByToken("EXBOSS_DIALOGUE_FORMAT", "{0}", self.GetLocalizedStringByToken("BROTHER_BODY_NAME"));
            return orig(self, token);
        }

        private void Phase1_PreEncounterBegin(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_PreEncounterBegin orig, EntityStates.Missions.BrotherEncounter.Phase1 self)
        {
            orig(self);
            EXBossAssets.fightEffectsActive = true;
            if (BrotherEXAssets.arenaEffectsPrefab)
                arenaEffects = Object.Instantiate(BrotherEXAssets.arenaEffectsPrefab, new Vector3(-80f, 491f, 0f), Quaternion.identity);
        }

        private void EncounterFinished_OnEnter(On.EntityStates.Missions.BrotherEncounter.EncounterFinished.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.EncounterFinished self)
        {
            orig(self);
            EXBossAssets.fightEffectsActive = false;
            if (arenaEffects) Object.Destroy(arenaEffects);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            CharacterBody.onBodyStartGlobal -= CharacterBody_onBodyStartGlobal;
            On.RoR2.CharacterBody.OnDeathStart -= CharacterBody_OnDeathStart;
            On.RoR2.MusicController.PickCurrentTrack -= MusicController_PickCurrentTrack;
            On.RoR2.Language.GetLocalizedStringByToken -= Language_GetLocalizedStringByToken;
            On.EntityStates.Missions.BrotherEncounter.Phase1.PreEncounterBegin -= Phase1_PreEncounterBegin;
            On.EntityStates.Missions.BrotherEncounter.EncounterFinished.OnEnter -= EncounterFinished_OnEnter;

            EntityStates.BrotherMonster.WeaponSlam.waveProjectileArc = oldWaveProjectileArc;
            EntityStates.BrotherMonster.WeaponSlam.waveProjectileCount -= 6;

            On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate -= WeaponSlam_FixedUpdate;
            On.EntityStates.BrotherMonster.SprintBash.OnEnter -= SprintBash_OnEnter;
            On.EntityStates.BrotherMonster.HoldSkyLeap.OnEnter -= HoldSkyLeap_OnEnter;
            On.EntityStates.BrotherMonster.HoldSkyLeap.FixedUpdate -= HoldSkyLeap_FixedUpdate;
            On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter -= ExitSkyLeap_OnEnter;
            EntityStates.BrotherMonster.BaseSlideState.duration /= 0.5f;
            On.EntityStates.BrotherMonster.UltChannelState.FireWave -= UltChannelState_FireWave;
            On.EntityStates.BrotherMonster.FistSlam.FixedUpdate -= FistSlam_FixedUpdate;
            On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter -= FireLunarShards_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter -= Phase2_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.Phase2.FixedUpdate -= Phase2_FixedUpdate;
            On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.FixedUpdate -= BrotherEncounterPhaseBaseState_FixedUpdate;

            EXBossAssets.fightEffectsActive = false;
        }

        public static void GlobalKnockup(Vector3 myPosition, bool strong)
        {
            foreach (var enemy in TeamComponent.GetTeamMembers(TeamIndex.Player))
            {
                if (enemy.body)
                {
                    Buffs.PlayerKnockupStun.KnockupBody(enemy.body, Vector3.up * 1000f + (myPosition - enemy.body.corePosition).normalized * (strong ? -3000f : 500f), 2.3f);
                }
            }
        }

        private void WeaponSlam_FixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.WeaponSlam self)
        {
            if (self.modelAnimator)
            {
                if (self.modelAnimator.GetFloat("blast.hitBoxActive") > 0.5f && !self.hasDoneBlastAttack)
                {
                    GlobalKnockup(self.characterBody.corePosition, PhaseCounter.instance && PhaseCounter.instance.phase == 3);
                }
            }
            orig(self);
        }

        private void SprintBash_OnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, EntityStates.BrotherMonster.SprintBash self)
        {
            orig(self);
            DamageAPI.AddModdedDamageType(self.overlapAttack, BrotherEXAssets.destroyItemDamageType);
        }

        private void HoldSkyLeap_OnEnter(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.HoldSkyLeap self)
        {
            var originalPosition = self.characterBody.transform.position;
            orig(self);

            if (NetworkServer.active)
            {
                var controller = Object.Instantiate(BrotherEXAssets.skyLeapHomingController);
                controller.transform.position = originalPosition;
                controller.GetComponent<BrotherEXAssets.ChallengeModeSkyLeapHomingController>().body = self.characterBody;
                NetworkServer.Spawn(controller);
            }
        }

        private void HoldSkyLeap_FixedUpdate(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_FixedUpdate orig, EntityStates.BrotherMonster.HoldSkyLeap self)
        {
            orig(self);
            self.fixedAge -= 0.3f * Time.fixedDeltaTime;
        }

        private void ExitSkyLeap_OnEnter(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, EntityStates.BrotherMonster.ExitSkyLeap self)
        {
            var oldDamage = EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileDamageCoefficient;
            EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileDamageCoefficient = 1f;
            orig(self);
            EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileDamageCoefficient = oldDamage;

            GlobalKnockup(self.characterBody.corePosition, false);
            foreach (var controller in InstanceTracker.GetInstancesList<BrotherEXAssets.ChallengeModeSkyLeapHomingController>().ToList())
            {
                if (controller.body == self.characterBody)
                {
                    Object.Destroy(controller.gameObject);
                }
            }
        }

        private void UltChannelState_FireWave(On.EntityStates.BrotherMonster.UltChannelState.orig_FireWave orig, EntityStates.BrotherMonster.UltChannelState self)
        {
            var oldDamage = EntityStates.BrotherMonster.UltChannelState.waveProjectileDamageCoefficient;
            EntityStates.BrotherMonster.UltChannelState.waveProjectileDamageCoefficient = 3f;
            orig(self);
            EntityStates.BrotherMonster.UltChannelState.waveProjectileDamageCoefficient = oldDamage;

            RoR2Application.fixedTimeTimers.CreateTimer(1f, () =>
            {
                if (self == null || !self.gameObject) return;
                GlobalKnockup(self.characterBody.corePosition, false);
                if (self.isAuthority)
                {
                    float num = 360f / (float)EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileCount;
                    Vector3 point = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
                    Vector3 footPosition = self.characterBody.footPosition;
                    for (int i = 0; i < EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileCount; i++)
                    {
                        Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * point;
                        ProjectileManager.instance.FireProjectile(EntityStates.BrotherMonster.ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * 1f, EntityStates.BrotherMonster.ExitSkyLeap.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master), DamageColorIndex.Default, null, -1f);
                    }
                }
            });
        }

        private void FistSlam_FixedUpdate(On.EntityStates.BrotherMonster.FistSlam.orig_FixedUpdate orig, EntityStates.BrotherMonster.FistSlam self)
        {
            if (self.modelAnimator)
            {
                if (self.modelAnimator.GetFloat("fist.hitBoxActive") > 0.5f && !self.hasAttacked)
                {
                    GlobalKnockup(self.characterBody.corePosition, false);
                }
            }
            orig(self);
        }

        private MeteorStormController _meteorStormController;
        public MeteorStormController meteorStormController
        {
            get
            {
                if (!_meteorStormController)
                    _meteorStormController = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm").GetComponent<MeteorStormController>();
                return _meteorStormController;
            }
        }

        public float meteorTimer;
        public void RunMeteors(float meteorInterval, int meteorCount, float meteorRadius, float meteorBaseDamage)
        {
            if (NetworkServer.active)
            {
                meteorTimer -= Time.fixedDeltaTime;
                if (meteorTimer <= 0f)
                {
                    meteorTimer += meteorInterval;

                    for (var i = 0; i < meteorCount; i++)
                    {
                        var circle = 250f * Random.insideUnitCircle;
                        var meteorPosition = new Vector3(circle.x, 491f, circle.y);
                        if (Physics.Raycast(new Ray(meteorPosition, Vector3.down), out var hitInfo, 500f, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
                            meteorPosition.y = hitInfo.point.y;
                        meteorPosition += Vector3.up * 0.2f;

                        RoR2Application.fixedTimeTimers.CreateTimer(Random.Range(0f, meteorInterval), () =>
                        {
                            if (!isActive) return;
                            EffectManager.SpawnEffect(meteorStormController.warningEffectPrefab, new EffectData
                            {
                                origin = meteorPosition,
                                scale = meteorRadius
                            }, true);
                            RoR2Application.fixedTimeTimers.CreateTimer(2f, () =>
                            {
                                if (!isActive) return;
                                EffectManager.SpawnEffect(meteorStormController.impactEffectPrefab, new EffectData
                                {
                                    origin = meteorPosition
                                }, true);
                                new BlastAttack
                                {
                                    baseDamage = meteorBaseDamage * (0.8f + 0.2f * Run.instance.ambientLevelFloor),
                                    crit = false,
                                    falloffModel = BlastAttack.FalloffModel.None,
                                    bonusForce = Vector3.zero,
                                    damageColorIndex = DamageColorIndex.Default,
                                    position = meteorPosition,
                                    procChainMask = default(ProcChainMask),
                                    procCoefficient = 1f,
                                    teamIndex = TeamIndex.Monster,
                                    radius = meteorRadius
                                }.Fire();
                            });
                        });
                    }
                }
            }
        }

        private void FireLunarShards_OnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, EntityStates.BrotherMonster.Weapon.FireLunarShards self)
        {
            orig(self);
            if (self.activatorSkillSlot.stock <= 0)
            {
                Util.PlaySound(EntityStates.LunarWisp.SeekingBomb.fireBombSoundString, self.gameObject);
                Ray aimRay = self.GetAimRay();
                Transform modelTransform = self.GetModelTransform();
                if (modelTransform)
                {
                    ChildLocator component = modelTransform.GetComponent<ChildLocator>();
                    if (component)
                    {
                        aimRay.origin = component.FindChild(EntityStates.BrotherMonster.Weapon.FireLunarShards.muzzleString).transform.position;
                    }
                }
                if (self.isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(
                        EntityStates.LunarWisp.SeekingBomb.projectilePrefab,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        self.gameObject,
                        self.damageStat * EntityStates.LunarWisp.SeekingBomb.bombDamageCoefficient,
                        EntityStates.LunarWisp.SeekingBomb.bombForce,
                        Util.CheckRoll(self.critStat, self.characterBody.master),
                        DamageColorIndex.Default,
                        null,
                        -1f
                    );
                }
            }
        }

        public float spinnyTimer;
        public void RunSpinny(float spinnyInterval, int count, float damageMultiplier)
        {
            spinnyTimer += Time.fixedDeltaTime;
            if (spinnyTimer >= spinnyInterval)
            {
                spinnyTimer -= spinnyInterval;

                if (brotherDummyAttackerMaster && brotherDummyAttackerMaster.hasBody)
                {
                    var brotherDummyAttackerBody = brotherDummyAttackerMaster.GetBody();
                    var spinnyPrefab = EntityStates.BrotherMonster.UltChannelState.waveProjectileLeftPrefab;
                    if (Random.value <= 0.5f)
                    {
                        spinnyPrefab = EntityStates.BrotherMonster.UltChannelState.waveProjectileRightPrefab;
                    }
                    var num = 360f / (float)count;
                    Vector3 normalized = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up).normalized;
                    Vector3 footPosition = new Vector3(-80f, 491f, 0f);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * normalized;
                        ProjectileManager.instance.FireProjectile(
                            spinnyPrefab,
                            footPosition,
                            Util.QuaternionSafeLookRotation(forward),
                            brotherDummyAttackerBody.gameObject,
                            brotherDummyAttackerBody.damage * damageMultiplier,
                            EntityStates.BrotherMonster.UltChannelState.waveProjectileForce,
                            false,
                            DamageColorIndex.Default,
                            null,
                            -1f
                        );
                    }
                }
            }
        }

        private void Phase2_OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.Phase2 self)
        {
            orig(self);
            if (!brotherDummyAttackerMaster && NetworkServer.active)
            {
                var directorSpawnRequest = new DirectorSpawnRequest(CharacterMasters.BrotherDummyAttacker.characterSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = new Vector3(0f, 1000f, 0f)
                }, RoR2Application.rng);
                directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.onSpawnedServer += (spawnResult) =>
                {
                    if (spawnResult.success && spawnResult.spawnedInstance)
                    {
                        brotherDummyAttackerMaster = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                        brotherDummyAttackerMaster.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
                    }
                };
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        private void Phase2_FixedUpdate(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_FixedUpdate orig, EntityStates.Missions.BrotherEncounter.Phase2 self)
        {
            orig(self);
            RunMeteors(1f, 15, 8f, 20f);
            RunSpinny(6f, 4, 2f);
        }

        private void BrotherEncounterPhaseBaseState_FixedUpdate(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_FixedUpdate orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
        {
            orig(self);
            if (PhaseCounter.instance)
            {
                if (PhaseCounter.instance.phase == 3) RunMeteors(0.2f, 6, 10f, 30f);
                if (PhaseCounter.instance.phase == 4) RunMeteors(0.2f, 12, 6f, 30f);
            }
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageNameMatches("moon2") && Util.CheckRoll(ChallengeModeConfig.specialModifierChance);
        }
    }
}