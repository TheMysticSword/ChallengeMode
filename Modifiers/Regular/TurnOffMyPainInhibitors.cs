using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class TurnOffMyPainInhibitors : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_TURNOFFMYPAININHIBITORS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_TURNOFFMYPAININHIBITORS_DESC";

        public static float damageMultiplier = 1.25f;
        public static float maxBuffDuration = 10f;
        public static float minDurationThreshold = 0.5f;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnApplyDamageIncreaseModifiers += GenericGameEvents_OnApplyDamageIncreaseModifiers;
            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;
        }

        private void GenericGameEvents_OnApplyDamageIncreaseModifiers(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo, ref float damage)
        {
            if (victimInfo.teamIndex == TeamIndex.Player)
                damage *= damageMultiplier;
        }

        private void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.victimBody && damageReport.victimTeamIndex == TeamIndex.Player)
            {
                var duration = maxBuffDuration * (damageReport.damageDealt / damageReport.victimBody.healthComponent.fullCombinedHealth);
                if (duration >= minDurationThreshold) damageReport.victimBody.AddTimedBuff(RoR2Content.Buffs.WarCryBuff, duration);
            }
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnApplyDamageIncreaseModifiers -= GenericGameEvents_OnApplyDamageIncreaseModifiers;
            GenericGameEvents.OnTakeDamage -= GenericGameEvents_OnTakeDamage;
        }
    }
}