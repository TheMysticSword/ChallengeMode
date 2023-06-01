using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class ExpensivePurchasables : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_EXPENSIVEPURCHASABLES_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_EXPENSIVEPURCHASABLES_DESC";

        public float chanceToMakeExpensive = 20f;
        public float costMultiplier = 1.5f;

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            foreach (var purchaseInteraction in InstanceTracker.GetInstancesList<PurchaseInteraction>())
            {
                if (NetworkServer.active && !purchaseInteraction.GetComponent<ChallengeModeExpensivePurchasable>() && Util.CheckRoll(chanceToMakeExpensive))
                {
                    purchaseInteraction.gameObject.AddComponent<ChallengeModeExpensivePurchasable>();
                    purchaseInteraction.Networkcost = (int)(purchaseInteraction.Networkcost * costMultiplier);
                }
            }
        }

        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            if (NetworkServer.active && !self.GetComponent<ChallengeModeExpensivePurchasable>() && Util.CheckRoll(chanceToMakeExpensive))
            {
                self.gameObject.AddComponent<ChallengeModeExpensivePurchasable>();
                self.Networkcost = (int)(self.Networkcost * costMultiplier);
            }
        }

        public class ChallengeModeExpensivePurchasable : MonoBehaviour { }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.PurchaseInteraction.Awake -= PurchaseInteraction_Awake;
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageHasCommonInteractables();
        }
    }
}