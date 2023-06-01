using MysticsRisky2Utils;
using RoR2;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class MissChance : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_MISSCHANCE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_MISSCHANCE_DESC";

        public float chance = 10f;
        
        public override void OnEnable()
        {
            base.OnEnable();
            GenericGameEvents.BeforeTakeDamage += GenericGameEvents_BeforeTakeDamage;
        }

        private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && !damageInfo.damageType.HasFlag(DamageType.BypassBlock) && attackerInfo.teamIndex == TeamIndex.Player && !Util.CheckRoll(100f - chance))
            {
                damageInfo.rejected = true;

                var effectData = new EffectData
                {
                    origin = damageInfo.position,
                    rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : Random.onUnitSphere)
                };
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData, true);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GenericGameEvents.BeforeTakeDamage -= GenericGameEvents_BeforeTakeDamage;
        }
    }
}