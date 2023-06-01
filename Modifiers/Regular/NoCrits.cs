using R2API;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class NoCrits : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_NOCRITS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_NOCRITS_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.teamComponent.teamIndex == TeamIndex.Player)
            {
                self.crit = 0f;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
        }
    }
}