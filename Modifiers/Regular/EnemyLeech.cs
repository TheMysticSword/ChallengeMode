using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class EnemyLeech : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_ENEMYLEECH_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_ENEMYLEECH_DESC";

        public float leechFraction = 0.33f;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;
        }

        private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (damageInfo.procCoefficient > 0 && damageInfo.damage > 0 && attackerInfo.teamIndex != TeamIndex.Player && victimInfo.teamIndex == TeamIndex.Player && attackerInfo.healthComponent)
            {
                attackerInfo.healthComponent.Heal(damageInfo.damage * leechFraction, default);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnHitEnemy -= GenericGameEvents_OnHitEnemy;
        }
    }
}