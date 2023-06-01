using R2API;

namespace ChallengeMode.Modifiers
{
    public class ReducedVision : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_REDUCEDVISION_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_REDUCEDVISION_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, RoR2.CharacterBody self)
        {
            orig(self);
            if (self.teamComponent.teamIndex == RoR2.TeamIndex.Player)
            {
                if (self.visionDistance >= float.PositiveInfinity)
                {
                    self.visionDistance = 45f;
                }
                else
                {
                    self.visionDistance *= 0.5f;
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
        }

        public override bool IsAvailable()
        {
            return !ChallengeModeUtils.CurrentStageNameMatches("voidraid");
        }
    }
}