using RoR2;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class PurchasesInflictSlow : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_PURCHASESINFLICTSLOW_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_PURCHASESINFLICTSLOW_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CostTypeDef.PayCost += CostTypeDef_PayCost;
        }

        private CostTypeDef.PayCostResults CostTypeDef_PayCost(On.RoR2.CostTypeDef.orig_PayCost orig, CostTypeDef self, int cost, Interactor activator, GameObject purchasedObject, Xoroshiro128Plus rng, ItemIndex avoidedItemIndex)
        {
            var result = orig(self, cost, activator, purchasedObject, rng, avoidedItemIndex);
            if (cost > 0)
            {
                var body = activator.GetComponent<CharacterBody>();
                if (body && body.teamComponent && body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    body.AddTimedBuff(RoR2Content.Buffs.Slow50, 8f);
                }
            }
            return result;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CostTypeDef.PayCost -= CostTypeDef_PayCost;
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageHasCommonInteractables() ||
                ChallengeModeUtils.CurrentStageNameMatches("voidstage") ||
                ChallengeModeUtils.CurrentStageNameMatches("arena") ||
                ChallengeModeUtils.CurrentStageNameMatches("bazaar");
        }
    }
}