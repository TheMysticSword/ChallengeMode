using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.Modifiers
{
    public class FlutterJumps : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_FLUTTERJUMPS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_FLUTTERJUMPS_DESC";

        public int extraJumps = 10;
        public float jumpPowerMultiplier = 0.45f;

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
                self.maxJumpCount += extraJumps;
                self.jumpPower *= jumpPowerMultiplier;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
        }
    }
}