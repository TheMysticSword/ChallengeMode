using MysticsRisky2Utils;
using RoR2;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class DefenseDamage : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_DEFENSEDAMAGE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_DEFENSEDAMAGE_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;
        }

        private void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.damageInfo.procCoefficient > 0 && damageReport.victimBody && damageReport.victimTeamIndex == TeamIndex.Player)
            {
                var debuffStacks = damageReport.damageDealt * 0.09f;
                for (var i = 0; i < Mathf.FloorToInt(debuffStacks); i++)
                    damageReport.victimBody.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_DefenseDamage, 10f);
            }
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnTakeDamage -= GenericGameEvents_OnTakeDamage;
        }
    }
}