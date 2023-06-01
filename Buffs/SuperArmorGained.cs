using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;

namespace ChallengeMode.Buffs
{
    public class SuperArmorGained : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_SuperArmorGained";
            buffDef.buffColor = new Color32(48, 165, 255, 255);
            buffDef.canStack = false;
            buffDef.isDebuff = false;
            buffDef.isHidden = true;
        }
    }
}
