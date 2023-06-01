using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;

namespace ChallengeMode.Buffs
{
    public class Frostbite : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_Frostbite";
            buffDef.buffColor = new Color32(153, 255, 246, 255);
            buffDef.canStack = true;
            buffDef.isDebuff = true;
            buffDef.iconSprite = ChallengeModePlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/ChallengeMode/Modifiers/Frostbite/texBuffFrostbite.png");
        }
    }
}
