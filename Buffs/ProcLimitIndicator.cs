using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.Buffs
{
    public class ProcLimitIndicator : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_ProcLimitIndicator";
            buffDef.buffColor = new Color32(201, 249, 255, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Nullifier/texBuffNullifyStackIcon.tif").WaitForCompletion();
        }
    }
}
