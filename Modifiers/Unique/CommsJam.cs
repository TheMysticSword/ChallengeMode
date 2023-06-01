using MysticsRisky2Utils;
using RoR2;
using RoR2.Artifacts;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Unique
{
    public class CommsJam : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_COMMSJAM_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_COMMSJAM_DESC";
        public override bool isAdditional => true;

        public float duration = 7f;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;
        }

        private void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (damageReport.damageInfo.procCoefficient > 0f && damageReport.victimBody && damageReport.victimTeamIndex == TeamIndex.Player)
            {
                damageReport.victimBody.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_CommsJammed, duration * damageReport.damageInfo.procCoefficient);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnTakeDamage -= GenericGameEvents_OnTakeDamage;
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageNameMatches("shipgraveyard") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierChance);
        }
    }
}