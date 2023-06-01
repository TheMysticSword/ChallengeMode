using R2API;
using RoR2;
using MysticsRisky2Utils;

namespace ChallengeMode.Modifiers
{
    public class BossFocus : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BOSSFOCUS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BOSSFOCUS_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (!sender.isBoss && sender.teamComponent.teamIndex != TeamIndex.Player && sender.teamComponent.teamIndex != TeamIndex.Neutral && BossGroup.GetTotalBossCount() > 0)
            {
                args.armorAdd += 60f;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageHasBosses();
        }
    }
}