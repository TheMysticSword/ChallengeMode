using RoR2;

namespace ChallengeMode.Modifiers
{
    public class KillRemorse : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_KILLREMORSE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_KILLREMORSE_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victimTeamIndex != TeamIndex.Player && damageReport.victimTeamIndex != TeamIndex.Neutral && damageReport.attackerTeamIndex == TeamIndex.Player && damageReport.attackerBody)
            {
                damageReport.attackerBody.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_KillRemorse, 20f);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }
    }
}