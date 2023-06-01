using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Unique
{
    public class Thunder : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_THUNDER_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_THUNDER_DESC";
        public override bool isAdditional => true;

        public static GameObject warningEffectPrefab;
        public static GameObject delayedProjectileSpawner;
        public static GameObject projectilePrefab;

        public static CharacterMaster thunderAttackerMaster;

        public float timer = 0f;
        public float intervalMin = 3f;
        public float intervalMax = 9f;
        public float waveTimerMin = 0f;
        public float waveTimerMax = 5f;

        public int targets = 3;
        public bool canStrikeSameTarget = true;
        
        public float raycastUpOffset = 10f;
        public float raycastDownRadius = 1f;

        public static float damageMultiplier = 8f;

        public class LightningStrikeWave
        {
            public Transform targetTransform;
            public float timer;
        }
        public List<LightningStrikeWave> lightningStrikeWaves = new List<LightningStrikeWave>();

        public static void InitAssets()
        {
            warningEffectPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/Thunder/LightningWarning.prefab");
            warningEffectPrefab.AddComponent<EffectComponent>();
            warningEffectPrefab.AddComponent<DestroyOnTimer>().duration = 2f;
            var vfxAttributes = warningEffectPrefab.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
            // var component = warningEffectPrefab.AddComponent<ChallengeModeThunderEffect>();
            // component.flashingBoltSystem = component.transform.Find("PoleLightning").GetComponent<ParticleSystem>();
            var flickerLight = warningEffectPrefab.transform.Find("Point Light").gameObject.AddComponent<FlickerLight>();
            flickerLight.light = flickerLight.GetComponent<Light>();
            flickerLight.sinWaves = new Wave[]
            {
                new Wave
                {
                    amplitude = 1f,
                    frequency = 0.1f
                },
                new Wave
                {
                    amplitude = 1f,
                    frequency = 2f
                },
                new Wave
                {
                    amplitude = 1f,
                    frequency = 0.7f
                }
            };
            ChallengeModeContent.Resources.effectPrefabs.Add(warningEffectPrefab);

            delayedProjectileSpawner = MysticsRisky2Utils.Utils.CreateBlankPrefab("ChallengeMode_ThunderDelayedProjectileSpawner");
            delayedProjectileSpawner.AddComponent<ChallengeModeThunderDelayedProjectileSpawner>();

            if (!projectilePrefab)
                projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ElectricWorm/ElectricOrbProjectile.prefab").WaitForCompletion();
        }

        public class ChallengeModeThunderEffect : MonoBehaviour
        {
            public float flashingBoltDelay = 1.7f;
            public bool flashingBoltSpawned = false;
            public ParticleSystem flashingBoltSystem;

            public void FixedUpdate()
            {
                if (!flashingBoltSpawned)
                {
                    flashingBoltDelay -= Time.fixedDeltaTime;
                    if (flashingBoltDelay <= 0f)
                    {
                        flashingBoltSpawned = true;
                        if (flashingBoltSystem) flashingBoltSystem.Play();
                    }
                }
            }
        }

        public class ChallengeModeThunderDelayedProjectileSpawner : MonoBehaviour
        {
            public float delay = 2f;

            public void FixedUpdate()
            {
                delay -= Time.fixedDeltaTime;
                if (delay <= 0f)
                {
                    if (thunderAttackerMaster && thunderAttackerMaster.hasBody)
                    {
                        var body = thunderAttackerMaster.GetBody();
                        ProjectileManager.instance.FireProjectile(
                            projectilePrefab,
                            transform.position + Vector3.up * 1f,
                            Util.QuaternionSafeLookRotation(Vector3.down),
                            body.gameObject,
                            body.damage * damageMultiplier,
                            0f,
                            false
                        );
                    }

                    Destroy(gameObject);
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;

            if (!thunderAttackerMaster && NetworkServer.active)
            {
                var directorSpawnRequest = new DirectorSpawnRequest(CharacterMasters.ThunderAttacker.characterSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = Vector3.one * -9999f
                }, RoR2Application.rng);
                directorSpawnRequest.teamIndexOverride = TeamIndex.Neutral;
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.onSpawnedServer += (spawnResult) =>
                {
                    if (spawnResult.success && spawnResult.spawnedInstance)
                    {
                        thunderAttackerMaster = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                        thunderAttackerMaster.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
                    }
                };
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        private void RoR2Application_onFixedUpdate()
        {
            if (NetworkServer.active)
            {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                {
                    timer += RoR2Application.rng.RangeFloat(intervalMin, intervalMax);

                    var bodies = CharacterBody.readOnlyInstancesList.Where(x => x.teamComponent.teamIndex != TeamIndex.Neutral).ToList();
                    for (var i = 0; i < targets; i++)
                    {
                        if (bodies.Count <= 0) break;
                        var j = RoR2Application.rng.RangeInt(0, bodies.Count);
                        var targetBody = bodies[j];
                        if (!canStrikeSameTarget) bodies.RemoveAt(j);

                        lightningStrikeWaves.Add(new LightningStrikeWave
                        {
                            targetTransform = targetBody.transform,
                            timer = RoR2Application.rng.RangeFloat(waveTimerMin, waveTimerMax)
                        });
                    }
                }

                for (var i = lightningStrikeWaves.Count - 1; i >= 0; i--)
                {
                    var lightningStrikeWave = lightningStrikeWaves[i];
                    lightningStrikeWave.timer -= Time.fixedDeltaTime;
                    if (lightningStrikeWave.timer <= 0)
                    {
                        lightningStrikeWaves.RemoveAt(i);

                        if (lightningStrikeWave.targetTransform)
                        {
                            var downDirection = Random.onUnitSphere;
                            downDirection.y = -1f;
                            downDirection *= raycastDownRadius;
                            var ray = new Ray(lightningStrikeWave.targetTransform.position + Vector3.up * raycastUpOffset, downDirection);
                            if (Physics.Raycast(ray, out var hitInfo, 100f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                            {
                                Object.Instantiate(delayedProjectileSpawner, hitInfo.point, Quaternion.identity);

                                EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
                                {
                                    origin = hitInfo.point,
                                    rotation = Util.QuaternionSafeLookRotation(Vector3.forward, hitInfo.normal)
                                }, true);
                            }
                        }
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RoR2Application.onFixedUpdate -= RoR2Application_onFixedUpdate;
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageNameMatches("rootjungle") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierChance);
        }
    }
}