using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace ChallengeMode.Buffs
{
    public class DefenseDamage : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_DefenseDamage";
            buffDef.buffColor = new Color32(99, 33, 29, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = true;
            buffDef.isHidden = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/EclipseRun/bdPermanentCurse.asset").WaitForCompletion().iconSprite;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                args.armorAdd -= 1f * sender.GetBuffCount(buffDef);
            }
        }
    }
}
