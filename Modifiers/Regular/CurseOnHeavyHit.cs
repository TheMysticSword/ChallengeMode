using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class CurseOnHeavyHit : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_CURSEONHEAVYHIT_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_CURSEONHEAVYHIT_DESC";

        public float duration = 10f;
        public int cooldown = 20;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;
        }

        private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (damageInfo.procCoefficient > 0f && victimInfo.body && attackerInfo.body && attackerInfo.teamIndex == TeamIndex.Player && !attackerInfo.body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_CurseCooldown) && (damageInfo.damage / attackerInfo.body.damage) >= 4f)
            {
                victimInfo.body.AddTimedBuff(RoR2Content.Buffs.DeathMark, duration);
                attackerInfo.body.AddTimedBuff(RoR2Content.Buffs.DeathMark, duration);
                for (var i = 0; i < cooldown; i++)
                {
                    attackerInfo.body.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_CurseCooldown, i + 1);
                }
            }
        }
        
        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnHitEnemy -= GenericGameEvents_OnHitEnemy;
        }
    }
}