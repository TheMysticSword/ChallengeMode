using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;

namespace ChallengeMode.Buffs
{
    public class CommsJammedVisuals : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_CommsJammedVisuals";
            buffDef.buffColor = new Color32(203, 214, 213, 255);
            buffDef.canStack = false;
            buffDef.isDebuff = false;
            buffDef.isHidden = true;
        }
    }
}
