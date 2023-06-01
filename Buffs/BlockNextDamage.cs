using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.Buffs
{
    public class BlockNextDamage : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_BlockNextDamage";
            buffDef.buffColor = new Color32(201, 249, 255, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdArmorBoost.asset").WaitForCompletion().iconSprite;

            GenericGameEvents.BeforeTakeDamage += GenericGameEvents_BeforeTakeDamage;
        }

        private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && !damageInfo.damageType.HasFlag(DamageType.BypassBlock) && victimInfo.body && victimInfo.body.HasBuff(buffDef))
            {
                victimInfo.body.RemoveBuff(buffDef);
                damageInfo.rejected = true;

                var effectData = new EffectData
                {
                    origin = damageInfo.position,
                    rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : Random.onUnitSphere)
                };
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData, true);
            }
        }
    }
}
