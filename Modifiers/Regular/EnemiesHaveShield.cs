using R2API;

namespace ChallengeMode.Modifiers
{
    public class EnemiesHaveShield : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_ENEMIESHAVESHIELD_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_ENEMIESHAVESHIELD_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(RoR2.CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.teamComponent.teamIndex != RoR2.TeamIndex.Player && sender.teamComponent.teamIndex != RoR2.TeamIndex.Neutral)
            {
                args.baseShieldAdd += sender.maxHealth * sender.cursePenalty * 0.1f;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }
    }
}