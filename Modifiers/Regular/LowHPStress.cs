using MysticsRisky2Utils;
using RoR2;

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
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
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

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            var result = orig(self, amount, procChainMask, nonRegen);
            if (self.body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_LowHPStress) && self.combinedHealthFraction >= 0.25f)
            {
                self.body.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_LowHPStress);
            }
            return result;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnTakeDamage -= GenericGameEvents_OnTakeDamage;
            On.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
        }

        public override bool IsAvailable()
        {
            return !Buffs.LowHPStress.hookFailed;
        }
    }
}