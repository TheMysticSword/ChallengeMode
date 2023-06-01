using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Buffs
{
    public class PlayerKnockupStun : BaseBuff
    {
        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            NetworkingAPI.RegisterMessageType<SyncKnockupToServer>();
        }

        public override void OnLoad() {
            buffDef.name = "ChallengeMode_PlayerKnockupStun";
            buffDef.buffColor = Color.white;
            buffDef.canStack = false;
            buffDef.isDebuff = true;
            buffDef.isHidden = true;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.GlobalEventManager.OnCharacterHitGroundServer += GlobalEventManager_OnCharacterHitGroundServer;
            On.RoR2.CharacterBody.OnClientBuffsChanged += CharacterBody_OnClientBuffsChanged;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                args.moveSpeedRootCount++;

                if (sender.skillLocator.primary) sender.skillLocator.primary.SetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
                if (sender.skillLocator.secondary) sender.skillLocator.secondary.SetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
                if (sender.skillLocator.utility) sender.skillLocator.utility.SetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
                if (sender.skillLocator.special) sender.skillLocator.special.SetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
            else
            {
                if (sender.skillLocator.primary) sender.skillLocator.primary.UnsetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
                if (sender.skillLocator.secondary) sender.skillLocator.secondary.UnsetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
                if (sender.skillLocator.utility) sender.skillLocator.utility.UnsetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
                if (sender.skillLocator.special) sender.skillLocator.special.UnsetSkillOverride(sender, BrotherEXAssets.playerStunLockedSkill, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(buffDef))
            {
                self.jumpPower = 0f;
                self.maxJumpHeight = 0f;
                self.maxJumpCount = 0;
                self.moveSpeed = 0f;
            }
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == this.buffDef) self.MarkAllStatsDirty();
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == this.buffDef) self.MarkAllStatsDirty();
        }

        private void GlobalEventManager_OnCharacterHitGroundServer(On.RoR2.GlobalEventManager.orig_OnCharacterHitGroundServer orig, GlobalEventManager self, CharacterBody characterBody, Vector3 impactVelocity)
        {
            orig(self, characterBody, impactVelocity);
            if (characterBody.HasBuff(buffDef))
            {
                characterBody.ClearTimedBuffs(buffDef);
                if (characterBody.HasBuff(buffDef))
                    characterBody.RemoveBuff(buffDef);
                characterBody.AddTimedBuff(buffDef, 0.5f);
            }
        }

        private void CharacterBody_OnClientBuffsChanged(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
        {
            orig(self);
            var helper = self.GetComponent<ChallengeModePlayerStunHelper>();
            if (!helper) helper = self.gameObject.AddComponent<ChallengeModePlayerStunHelper>();
            if (self.HasBuff(buffDef) && !helper.effectSpawned && helper.effectCooldown <= 0f)
            {
                helper.SpawnVFX(1.2f);
                helper.effectSpawned = true;
                helper.effectCooldown = 1f;
            }
            else if (!self.HasBuff(buffDef) && helper.effectSpawned)
            {
                helper.effectSpawned = false;
                helper.effectCooldown = 1f;
            }
        }

        public class ChallengeModePlayerStunHelper : MonoBehaviour
        {
            public CharacterBody body;
            public bool effectSpawned = false;
            public float effectCooldown = 0f;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
            }

            public void FixedUpdate()
            {
                effectCooldown -= Time.fixedDeltaTime;
            }

            public void SpawnVFX(float duration)
            {
                var stunVfxInstance = Object.Instantiate(EntityStates.StunState.stunVfxPrefab, body.transform);
                stunVfxInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
                stunVfxInstance.AddComponent<DestroyOnTimer>().duration = duration;
            }
        }

        public static void KnockupBody(CharacterBody body, Vector3 force, bool requireGrounded = true)
        {
            if (body && body.hasEffectiveAuthority && (!requireGrounded || (body.characterMotor && body.characterMotor.isGrounded)) && body.healthComponent)
            {
                HandleKnockupLocal(body, force);

                if (NetworkServer.active)
                {
                    body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_PlayerKnockupStun);
                }
                else
                {
                    new SyncKnockupToServer(body.GetComponent<NetworkIdentity>().netId).Send(NetworkDestination.Server);
                }
            }
        }

        private static void HandleKnockupLocal(CharacterBody body, Vector3 force)
        {
            body.characterMotor.velocity = Vector3.zero;
            body.healthComponent.TakeDamageForce(force, true, true);
            body.characterMotor.jumpCount = 0;
        }

        public class SyncKnockupToServer : INetMessage
        {
            NetworkInstanceId objID;

            public SyncKnockupToServer()
            {
            }

            public SyncKnockupToServer(NetworkInstanceId objID)
            {
                this.objID = objID;
            }

            public void Deserialize(NetworkReader reader)
            {
                objID = reader.ReadNetworkId();
            }

            public void OnReceived()
            {
                if (!NetworkServer.active) return;
                GameObject obj = Util.FindNetworkObject(objID);
                if (obj)
                {
                    var body = obj.GetComponent<CharacterBody>();
                    if (body)
                    {
                        body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_PlayerKnockupStun);
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(objID);
            }
        }
    }
}
