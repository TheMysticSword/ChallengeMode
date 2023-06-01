using MysticsRisky2Utils;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class EnemyMissileOnHit : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_ENEMYMISSILEONHIT_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_ENEMYMISSILEONHIT_DESC";

        public float chance = 10f;
        public float baseDamage = 0.3f;

        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;
        }

        private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (damageInfo.procCoefficient > 0 && attackerInfo.teamIndex == TeamIndex.Player && victimInfo.body && victimInfo.master && Util.CheckRoll(chance * damageInfo.procCoefficient, victimInfo.master))
            {
                MissileUtils.FireMissile(victimInfo.body.corePosition, victimInfo.body, new ProcChainMask(), attackerInfo.gameObject, baseDamage * victimInfo.body.damage, victimInfo.body.RollCrit(), GlobalEventManager.CommonAssets.missilePrefab, DamageColorIndex.Item, true);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.OnHitEnemy -= GenericGameEvents_OnHitEnemy;
        }
    }
}