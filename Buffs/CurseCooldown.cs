using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.Buffs
{
    public class CurseCooldown : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_CurseCooldown";
            buffDef.buffColor = new Color32(155, 155, 155, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = false;
            buffDef.isCooldown = true;
            buffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/DeathMark/bdDeathMark.asset").WaitForCompletion().iconSprite;
        }
    }
}
