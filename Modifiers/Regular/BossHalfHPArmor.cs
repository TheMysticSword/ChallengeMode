using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class BossHalfHPArmor : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BOSSHALFHPARMOR_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BOSSHALFHPARMOR_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;
        }

        private void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.victimBody && damageReport.victimBody.isBoss && damageReport.victimBody.healthComponent)
            {
                if (damageReport.victimBody.healthComponent.combinedHealthFraction <= 0.5f)
                {
                    if (!damageReport.victimBody.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_SuperArmorGained))
                    {
                        damageReport.victimBody.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_SuperArmorGained);
                        damageReport.victimBody.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_SuperArmor, 20f);
                    }
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
            return ChallengeModeUtils.CurrentStageHasBosses();
        }
    }
}