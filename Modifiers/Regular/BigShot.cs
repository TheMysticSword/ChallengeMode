using R2API;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class BigShot : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BIGSHOT_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BIGSHOT_DESC";

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
                self.attackSpeed *= 0.5f;
                self.damage *= 2f;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
        }
    }
}