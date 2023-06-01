using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class PrinterBug : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_PRINTERBUG_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_PRINTERBUG_DESC";

        public float triggerChance = 40f;

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            var changed = false;
            if (self.costType == CostTypeIndex.WhiteItem || self.costType == CostTypeIndex.GreenItem || self.costType == CostTypeIndex.RedItem || self.costType == CostTypeIndex.BossItem)
            {
                if (Util.CheckRoll(triggerChance))
                {
                    changed = true;
                    self.cost += 1;
                    if (!self.CanBeAffordedByInteractor(activator))
                    {
                        self.cost -= 1;
                        changed = false;
                    }
                }
            }
            orig(self, activator);
            if (changed) self.cost -= 1;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
        }

        public override bool IsAvailable()
        {
            if ((RunArtifactManager.instance &&
                RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.commandArtifactDef)) ||
                !ChallengeModeUtils.CurrentStageHasCommonInteractables())
            {
                return false;
            }
            return true;
        }
    }
}