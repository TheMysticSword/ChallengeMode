using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class IncreasedKnockback : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_INCREASEDKNOCKBACK_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_INCREASEDKNOCKBACK_DESC";

        public float knockbackMultiplier = 3f;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnApplyDamageIncreaseModifiers += GenericGameEvents_OnApplyDamageIncreaseModifiers;
        }

        private void GenericGameEvents_OnApplyDamageIncreaseModifiers(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo, ref float damage)
        {
            if (victimInfo.teamIndex == TeamIndex.Player)
                damageInfo.force *= knockbackMultiplier;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnApplyDamageIncreaseModifiers -= GenericGameEvents_OnApplyDamageIncreaseModifiers;
        }
    }
}