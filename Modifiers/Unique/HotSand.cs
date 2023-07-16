using RoR2;
using RoR2.Artifacts;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Unique
{
    public class HotSand : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_HOTSAND_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_HOTSAND_DESC";
        public override bool isAdditional => true;

        public static int overheatThreshold = 4;
        public static float overheatDuration = 5f;
        
        public static CharacterMaster fireInflictorMaster;

        public BodyIndex engiTurretBodyIndex;
        
        public override void OnEnable()
        {
            base.OnEnable();

            engiTurretBodyIndex = BodyCatalog.FindBodyIndex("EngiTurretBody");

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (!body.GetComponent<ChallengeModeHotSandHelper>())
                    body.gameObject.AddComponent<ChallengeModeHotSandHelper>();
            }

            if (!fireInflictorMaster && NetworkServer.active)
            {
                var directorSpawnRequest = new DirectorSpawnRequest(CharacterMasters.HotSandBurnInflictor.characterSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = new Vector3(-9999f, -9999f, -9999f)
                }, RoR2Application.rng);
                directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;
                directorSpawnRequest.ignoreTeamMemberLimit = true;
                directorSpawnRequest.onSpawnedServer += (spawnResult) =>
                {
                    if (spawnResult.success && spawnResult.spawnedInstance)
                    {
                        fireInflictorMaster = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
                        fireInflictorMaster.inventory.GiveItem(RoR2Content.Items.UseAmbientLevel);
                    }
                };
                DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
            }
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (!self.GetComponent<ChallengeModeHotSandHelper>())
            {
                if (self.isPlayerControlled || !self.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical) || self.bodyIndex == engiTurretBodyIndex)
                {
                    self.gameObject.AddComponent<ChallengeModeHotSandHelper>();
                }
            }
        }

        public class ChallengeModeHotSandHelper : MonoBehaviour
        {
            public CharacterBody body;
            public float overheatTime = 0f;
            public float overheatInterval = 0.5f;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
                if (body.isPlayerControlled) overheatTime = -4f;
            }

            public void FixedUpdate()
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player && NetworkServer.active)
                {
                    var heatGainRate = 1f;

                    if (ChallengeModeUtils.BodyIsHot(body)) heatGainRate += 0.2f;
                    if (ChallengeModeUtils.BodyIsCold(body)) heatGainRate -= 1.2f;

                    if (body.isSprinting) heatGainRate -= 1f;
                    else if (!body.GetNotMoving()) heatGainRate /= overheatDuration * 0.9f; // not sprinting but walking

                    overheatTime += heatGainRate * Time.fixedDeltaTime;
                    if (heatGainRate < 0 && overheatTime < -1f) overheatTime = -1f;
                    if (overheatTime >= overheatInterval)
                    {
                        overheatTime -= overheatInterval;

                        body.AddTimedBuff(RoR2Content.Buffs.Overheat, overheatDuration);
                        var overheatOverThreshold = body.GetBuffCount(RoR2Content.Buffs.Overheat) - overheatThreshold;
                        if (overheatOverThreshold >= 0 && fireInflictorMaster && fireInflictorMaster.hasBody)
                        {
                            var fireInflictorBody = fireInflictorMaster.GetBody();
                            
                            var totalDamage = 0.3f * fireInflictorBody.damage;
                            totalDamage *= (0.1f * overheatOverThreshold);

                            var inflictDotInfo = new InflictDotInfo();
                            inflictDotInfo.dotIndex = DotController.DotIndex.Burn;
                            inflictDotInfo.attackerObject = fireInflictorBody.gameObject;
                            inflictDotInfo.victimObject = body.gameObject;
                            inflictDotInfo.damageMultiplier = 1f;
                            inflictDotInfo.totalDamage = totalDamage;
                            DotController.InflictDot(ref inflictDotInfo);
                        }
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
                var helper = body.GetComponent<ChallengeModeHotSandHelper>();
                if (helper)
                    Object.Destroy(helper);
            }
        }

        public override bool IsAvailable()
        {
            return (ChallengeModeUtils.CurrentStageNameMatches("dampcavesimple") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierChance)) ||
                (ChallengeModeUtils.CurrentStageNameMatches("goolake") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierRareChance));
        }
    }
}