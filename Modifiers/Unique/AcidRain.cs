using RoR2;
using RoR2.Artifacts;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Unique
{
    public class AcidRain : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_ACIDRAIN_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_ACIDRAIN_DESC";
        public override bool isAdditional => true;

        public static GameObject rainPrefab;

        public GameObject rainEffect;

        public static float rainBaseDamage = 4f;
        public static float rainDamage = 0f;

        public override void OnEnable()
        {
            base.OnEnable();

            if (!rainPrefab)
            {
                rainPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/AcidRain/AcidRain.prefab");
            }

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (!body.GetComponent<ChallengeModeAcidRainHelper>())
                    body.gameObject.AddComponent<ChallengeModeAcidRainHelper>();
            }

            if (Run.instance) UpdateDamage(Run.instance);
            Run.onRunAmbientLevelUp += Run_onRunAmbientLevelUp;

            if (rainPrefab)
            {
                rainEffect = Object.Instantiate(rainPrefab);
                var weatherParticles = rainEffect.AddComponent<WeatherParticles>();
                weatherParticles.lockPosition = true;
                weatherParticles.lockRotation = false;
            }
        }

        private void Run_onRunAmbientLevelUp(Run run)
        {
            UpdateDamage(run);
        }

        public void UpdateDamage(Run run)
        {
            rainDamage = rainBaseDamage * (1f + 0.2f * (run.ambientLevel - 1f));
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (!self.GetComponent<ChallengeModeAcidRainHelper>())
                self.gameObject.AddComponent<ChallengeModeAcidRainHelper>();
        }

        public class ChallengeModeAcidRainHelper : MonoBehaviour
        {
            public CharacterBody body;
            public float rainTime = 0f;
            public float rainIntervalMin = 0.5f;
            public float rainIntervalMax = 1.5f;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
                if (body.isPlayerControlled) rainTime = -4f;
            }

            public void FixedUpdate()
            {
                if (body.isPlayerControlled && NetworkServer.active)
                {
                    rainTime -= Time.fixedDeltaTime;
                    if (rainTime <= 0f)
                    {
                        rainTime += RoR2Application.rng.RangeFloat(rainIntervalMin, rainIntervalMax);

                        if (!ChallengeModeUtils.IsBodyUnderCeiling(body))
                        {
                            if (body.healthComponent && body.healthComponent.alive && body.healthComponent.health > 1f)
                            {
                                var damageInfo = new DamageInfo();
                                damageInfo.damage = rainDamage;
                                damageInfo.attacker = null;
                                damageInfo.inflictor = null;
                                damageInfo.force = Vector3.down * 10f;
                                damageInfo.crit = false;
                                damageInfo.procCoefficient = 1f;
                                damageInfo.position = body.corePosition;
                                damageInfo.damageColorIndex = DamageColorIndex.Poison;
                                damageInfo.damageType = DamageType.NonLethal;

                                body.healthComponent.TakeDamage(damageInfo);
                                GlobalEventManager.instance.OnHitEnemy(damageInfo, body.healthComponent.gameObject);
                                GlobalEventManager.instance.OnHitAll(damageInfo, body.healthComponent.gameObject);
                            }
                        }

                        PostProcessing.ChallengeModePostProcessing.MarkDirtyForBody(body);
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                var helper = body.GetComponent<ChallengeModeAcidRainHelper>();
                if (helper)
                    Object.Destroy(helper);
            }
            Run.onRunAmbientLevelUp -= Run_onRunAmbientLevelUp;
            if (rainEffect) Object.Destroy(rainEffect);
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageNameMatches("sulfurpools") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierChance);
        }
    }
}