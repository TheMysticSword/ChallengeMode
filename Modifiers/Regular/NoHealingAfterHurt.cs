using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class NoHealingAfterHurt : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_NOHEALINGAFTERHURT_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_NOHEALINGAFTERHURT_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;
        }

        private void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.damageInfo.procCoefficient > 0 && damageReport.victimBody && damageReport.victimTeamIndex == TeamIndex.Player)
            {
                damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.HealingDisabled, 0.5f * damageReport.damageInfo.procCoefficient);
            }
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnTakeDamage -= GenericGameEvents_OnTakeDamage;
        }
    }
}