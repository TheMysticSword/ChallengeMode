using RoR2;
using System.Linq;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class HoldoutShrink : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_HOLDOUTSHRINK_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_HOLDOUTSHRINK_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
        }

        private void HoldoutZoneController_Start(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            orig(self);
            self.gameObject.AddComponent<ChallengeModeHoldoutShrinkHelper>();
        }

        public class ChallengeModeHoldoutShrinkHelper : MonoBehaviour
        {
            public HoldoutZoneController holdoutZoneController;

            public void Awake()
            {
                holdoutZoneController = GetComponent<HoldoutZoneController>();
                holdoutZoneController.calcRadius += HoldoutZoneController_calcRadius;
            }

            private void HoldoutZoneController_calcRadius(ref float radius)
            {
                radius += holdoutZoneController.baseRadius * 0.5f;
                radius /= 1f + 2f * holdoutZoneController.charge;
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
            On.RoR2.HoldoutZoneController.Start -= HoldoutZoneController_Start;
            foreach (var helper in InstanceTracker.GetInstancesList<ChallengeModeHoldoutShrinkHelper>().ToList())
            {
                Object.Destroy(helper);
            }
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageHasCommonInteractables() ||
                ChallengeModeUtils.CurrentStageNameMatches("arena") ||
                ChallengeModeUtils.CurrentStageNameMatches("voidstage") ||
                ChallengeModeUtils.CurrentStageNameMatches("moon2");
        }
    }
}