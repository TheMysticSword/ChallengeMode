using RoR2;
using RoR2.Artifacts;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Unique
{
    public class Frostbite : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_FROSTBITE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_FROSTBITE_DESC";
        public override bool isAdditional => true;

        public static GameObject effectPrefab;

        public static int maxTimeInCold = 100;
        public static float coldFlatDamage = 5f;
        public static float coldPercentDamage = 0.024f;
        public static int torchesToSpawn = 4;

        public override void OnEnable()
        {
            base.OnEnable();

            if (!effectPrefab)
            {
                effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXFrozen.prefab").WaitForCompletion();
            }

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (!body.GetComponent<ChallengeModeFrostbiteHelper>())
                    body.gameObject.AddComponent<ChallengeModeFrostbiteHelper>();
            }

            if (NetworkServer.active)
            {
                for (var i = 0; i < torchesToSpawn; i++)
                {
                    DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(Interactables.FrostbiteWarmthTorch.interactableSpawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    }, RoR2Application.rng));
                }
            }
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (!self.GetComponent<ChallengeModeFrostbiteHelper>())
                self.gameObject.AddComponent<ChallengeModeFrostbiteHelper>();
        }

        public class ChallengeModeFrostbiteHelper : MonoBehaviour
        {
            public CharacterBody body;
            public float coldTime = 0f;
            public float coldInterval = 1f;
            public float coldDamageTime = 0f;
            public float coldDamageInterval = 1f;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
                if (body.isPlayerControlled) coldTime = -4f;
            }

            public void FixedUpdate()
            {
                if (body.isPlayerControlled && NetworkServer.active)
                {
                    if (body.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_Frostbite) < maxTimeInCold)
                    {
                        var meterGainRate = 1f;

                        if (ChallengeModeUtils.BodyIsHot(body)) meterGainRate -= 0.2f;
                        if (ChallengeModeUtils.BodyIsCold(body)) meterGainRate += 0.2f;

                        coldTime += meterGainRate * Time.fixedDeltaTime;
                        if (coldTime >= coldInterval)
                        {
                            coldTime -= coldInterval;

                            body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_Frostbite);
                        }
                    }
                    else
                    {
                        coldDamageTime += Time.fixedDeltaTime;
                        if (coldDamageTime >= coldInterval)
                        {
                            coldDamageTime -= coldInterval;

                            if (body.healthComponent && body.healthComponent.alive)
                            {
                                var damageInfo = new DamageInfo();
                                damageInfo.damage = coldFlatDamage + coldPercentDamage * body.healthComponent.fullHealth;
                                damageInfo.attacker = null;
                                damageInfo.inflictor = null;
                                damageInfo.force = Vector3.zero;
                                damageInfo.crit = false;
                                damageInfo.procCoefficient = 1f;
                                damageInfo.position = body.corePosition;
                                damageInfo.damageColorIndex = Interactables.FrostbiteWarmthTorch.damageColorIndex;
                                damageInfo.damageType = DamageType.BypassArmor;

                                body.healthComponent.TakeDamage(damageInfo);
                                GlobalEventManager.instance.OnHitEnemy(damageInfo, body.healthComponent.gameObject);
                                GlobalEventManager.instance.OnHitAll(damageInfo, body.healthComponent.gameObject);

                                if (effectPrefab)
                                {
                                    EffectManager.SpawnEffect(effectPrefab, new EffectData
                                    {
                                        origin = body.corePosition,
                                        scale = 0.5f
                                    }, true);
                                }
                            }
                        }
                    }
                }
            }

            public void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            public void OnDisable()
            {
                InstanceTracker.Remove(this);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                var helper = body.GetComponent<ChallengeModeFrostbiteHelper>();
                if (helper)
                    Object.Destroy(helper);
            }
        }

        public override bool IsAvailable()
        {
            return (ChallengeModeUtils.CurrentStageNameMatches("frozenwall") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierChance)) ||
                (ChallengeModeUtils.CurrentStageNameMatches("snowyforest") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierRareChance));
        }
    }
}