using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;

namespace ChallengeMode.Buffs
{
    public class Stamina : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_Stamina";
            buffDef.buffColor = new Color32(251, 193, 22, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = false;
            buffDef.isHidden = false;
            buffDef.iconSprite = ChallengeModePlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/ChallengeMode/Modifiers/Stamina/texStaminaBuff.png");
        }
    }
}
