using R2API;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class PerfectedOnPlanet : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_PERFECTEDONPLANET_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_PERFECTEDONPLANET_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            HG.ArrayUtils.ArrayAppend(ref EliteAPI.VanillaEliteTiers[1].eliteTypes, RoR2Content.Elites.Lunar);
            HG.ArrayUtils.ArrayAppend(ref EliteAPI.VanillaEliteTiers[2].eliteTypes, RoR2Content.Elites.Lunar);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            ChallengeModeUtils.RemoveFromArray(ref EliteAPI.VanillaEliteTiers[1].eliteTypes, RoR2Content.Elites.Lunar);
            ChallengeModeUtils.RemoveFromArray(ref EliteAPI.VanillaEliteTiers[2].eliteTypes, RoR2Content.Elites.Lunar);
        }

        public override bool IsAvailable()
        {
            return !ChallengeModeUtils.CurrentStageNameMatches("moon2");
        }
    }
}