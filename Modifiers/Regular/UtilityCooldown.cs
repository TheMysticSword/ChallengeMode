using R2API;
using RoR2;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class UtilityCooldown : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_UTILITYCOOLDOWN_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_UTILITYCOOLDOWN_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.GenericSkill.CalculateFinalRechargeInterval += GenericSkill_CalculateFinalRechargeInterval;
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player && body.skillLocator.utilityBonusStockSkill)
                {
                    body.skillLocator.utilityBonusStockSkill.RecalculateFinalRechargeInterval();
                }
            }
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.teamComponent.teamIndex == TeamIndex.Player && self.skillLocator.utilityBonusStockSkill)
            {
                self.skillLocator.utilityBonusStockSkill.RecalculateFinalRechargeInterval();
            }
        }

        private float GenericSkill_CalculateFinalRechargeInterval(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, RoR2.GenericSkill self)
        {
            var result = orig(self);
            var body = self.GetComponent<CharacterBody>();
            if (body && body.teamComponent.teamIndex == TeamIndex.Player && (body.skillLocator.utilityBonusStockSkill == self || body.skillLocator.utility == self))
            {
                result += 10f;
            }
            return result;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.GenericSkill.CalculateFinalRechargeInterval -= GenericSkill_CalculateFinalRechargeInterval;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (body.teamComponent.teamIndex == TeamIndex.Player && body.skillLocator.utilityBonusStockSkill)
                {
                    body.skillLocator.utilityBonusStockSkill.RecalculateFinalRechargeInterval();
                }
            }
        }
    }
}