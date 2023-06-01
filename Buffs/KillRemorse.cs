using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace ChallengeMode.Buffs
{
    public class KillRemorse : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_KillRemorse";
            buffDef.buffColor = new Color32(127, 127, 127, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = true;
            buffDef.isHidden = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Bandit2/bdSuperBleed.asset").WaitForCompletion().iconSprite;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                args.baseCurseAdd += 0.02f * sender.GetBuffCount(buffDef);
            }
        }
    }
}
