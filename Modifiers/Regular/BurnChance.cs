using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class BurnChance : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BURNCHANCE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BURNCHANCE_DESC";

        public float chance = 20f;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;
        }

        private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (damageInfo.procCoefficient > 0 && attackerInfo.teamIndex != TeamIndex.Player && victimInfo.teamIndex == TeamIndex.Player && Util.CheckRoll(chance * damageInfo.procCoefficient, attackerInfo.master))
            {
                uint? maxStacksFromAttacker = null;
                if ((damageInfo != null) ? damageInfo.inflictor : null)
                {
                    RoR2.Projectile.ProjectileDamage component = damageInfo.inflictor.GetComponent<RoR2.Projectile.ProjectileDamage>();
                    if (component && component.useDotMaxStacksFromAttacker)
                    {
                        maxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
                    }
                }

                var inflictDotInfo = new InflictDotInfo
                {
                    attackerObject = damageInfo.attacker,
                    victimObject = victimInfo.gameObject,
                    totalDamage = new float?(damageInfo.damage * 0.5f),
                    damageMultiplier = 1f,
                    dotIndex = DotController.DotIndex.Burn,
                    maxStacksFromAttacker = maxStacksFromAttacker
                };
                if (attackerInfo.inventory)
                    StrengthenBurnUtils.CheckDotForUpgrade(attackerInfo.inventory, ref inflictDotInfo);
                DotController.InflictDot(ref inflictDotInfo);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnHitEnemy -= GenericGameEvents_OnHitEnemy;
        }
    }
}