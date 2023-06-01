using MysticsRisky2Utils;

namespace ChallengeMode.Modifiers
{
    public class LowHPStress : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_LOWHPSTRESS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_LOWHPSTRESS_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;
        }

        private void GenericGameEvents_OnTakeDamage(RoR2.DamageReport damageReport)
        {
            if (damageReport.victimBody && damageReport.victimTeamIndex == RoR2.TeamIndex.Player && damageReport.victimBody.healthComponent)
            {
                if (damageReport.victimBody.healthComponent.combinedHealthFraction < 0.25f)
                {
                    if (!damageReport.victimBody.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_LowHPStress))
                        damageReport.victimBody.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_LowHPStress);
                }
            }
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnTakeDamage -= GenericGameEvents_OnTakeDamage;
        }

        public override bool IsAvailable()
        {
            return !Buffs.LowHPStress.hookFailed;
        }
    }
}