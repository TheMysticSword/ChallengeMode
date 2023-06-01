namespace ChallengeMode.Modifiers
{
    public class AlwaysSlippery : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_ALWAYSSLIPPERY_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_ALWAYSSLIPPERY_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterMotor.OnGroundHit += CharacterMotor_OnGroundHit;
        }

        private void CharacterMotor_OnGroundHit(On.RoR2.CharacterMotor.orig_OnGroundHit orig, RoR2.CharacterMotor self, UnityEngine.Collider hitCollider, UnityEngine.Vector3 hitNormal, UnityEngine.Vector3 hitPoint, ref KinematicCharacterController.HitStabilityReport hitStabilityReport)
        {
            orig(self, hitCollider, hitNormal, hitPoint, ref hitStabilityReport);
            if (self.body && self.body.teamComponent.teamIndex == RoR2.TeamIndex.Player)
            {
                self.isAirControlForced = true;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterMotor.OnGroundHit -= CharacterMotor_OnGroundHit;
        }
    }
}