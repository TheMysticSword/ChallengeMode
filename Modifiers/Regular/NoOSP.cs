using R2API;

namespace ChallengeMode.Modifiers
{
    public class NoOSP : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_NOOSP_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_NOOSP_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.hasOneShotProtection)
            {
                args.baseShieldAdd += sender.maxHealth * sender.cursePenalty * sender.oneShotProtectionFraction;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }
    }
}