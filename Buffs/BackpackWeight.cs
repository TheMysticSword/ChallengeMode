using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace ChallengeMode.Buffs
{
    public class BackpackWeight : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_BackpackWeight";
            buffDef.buffColor = new Color32(193, 102, 46, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = true;
            buffDef.isHidden = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdSlow50.asset").WaitForCompletion().iconSprite;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                args.moveSpeedReductionMultAdd += 0.01f * sender.GetBuffCount(buffDef);
            }
        }
    }
}
