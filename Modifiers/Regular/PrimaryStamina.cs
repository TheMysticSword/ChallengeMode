using UnityEngine.Networking;
using RoR2;
using UnityEngine;
using System.Linq;

namespace ChallengeMode.Modifiers
{
    public class PrimaryStamina : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_PRIMARYSTAMINA_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_PRIMARYSTAMINA_DESC";

        public static float maxStamina = 40;
        public static float rechargeInterval = 0.125f;
        public static float rechargeDelay = 1f;
        public static float rechargeDelayAfterFullLoss = 4f;
        
        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.isPlayerControlled)
            {
                self.gameObject.AddComponent<ChallengeModeStaminaHelper>();
                for (var i = 0; i < maxStamina; i++)
                {
                    self.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_Stamina);
                }
            }
        }

        private void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            orig(self, skill);
            if (NetworkServer.active && self.skillLocator && (self.skillLocator.primary == skill || self.skillLocator.primaryBonusStockSkill == skill))
            {
                if (self.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_Stamina) && Util.CheckRoll(100f / self.attackSpeed))
                    self.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_Stamina);

                foreach (var helper in InstanceTracker.GetInstancesList<ChallengeModeStaminaHelper>())
                {
                    if (helper.body == self)
                    {
                        if (self.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_Stamina))
                            helper.delayTimer = rechargeDelay;
                        else
                            helper.delayTimer = rechargeDelayAfterFullLoss;
                    }
                }
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.isPlayerControlled && !self.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_Stamina))
            {
                self.attackSpeed *= 0.5f;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.OnSkillActivated -= CharacterBody_OnSkillActivated;
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            foreach (var helper in InstanceTracker.GetInstancesList<ChallengeModeStaminaHelper>().ToList())
            {
                Object.Destroy(helper);
            }
        }

        public class ChallengeModeStaminaHelper : MonoBehaviour
        {
            public CharacterBody body;
            public float delayTimer = 0f;
            public float rechargeTimer = 0f;

            public void Awake()
            {
                body = GetComponent<CharacterBody>();
            }

            public void FixedUpdate()
            {
                delayTimer -= Time.fixedDeltaTime;
                if (delayTimer <= 0f)
                {
                    rechargeTimer -= Time.fixedDeltaTime;
                    if (rechargeTimer <= 0f)
                    {
                        rechargeTimer += rechargeInterval;
                        if (body.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_Stamina) < maxStamina)
                        {
                            body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_Stamina);
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
    }
}