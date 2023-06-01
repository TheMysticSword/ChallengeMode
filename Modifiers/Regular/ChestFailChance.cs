using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class ChestFailChance : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_CHESTFAILCHANCE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_CHESTFAILCHANCE_DESC";

        public float chanceToFail = 33f;

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (self.CanBeAffordedByInteractor(activator) && self.displayNameToken.Contains("CHEST") && self.costType == CostTypeIndex.Money && self.GetComponent<ChestBehavior>())
            {
                var body = activator.GetComponent<CharacterBody>();
                if (body.master && !Util.CheckRoll(100f - chanceToFail))
                {
                    var costTypeDef = CostTypeCatalog.GetCostTypeDef(self.costType);
                    costTypeDef.PayCost(self.cost, activator, self.gameObject, self.rng, ItemIndex.None);

                    // play the purchase fail sound
                    activator.CallRpcInteractionResult(false);

                    return;
                }
            }
            orig(self, activator);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
        }

        public override bool IsAvailable()
        {
            if ((RunArtifactManager.instance &&
                RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.sacrificeArtifactDef)) ||
                !ChallengeModeUtils.CurrentStageHasCommonInteractables())
            {
                return false;
            }
            return true;
        }
    }
}