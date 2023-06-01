using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
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

            On.RoR2.Util.CheckRoll_float_CharacterMaster += Util_CheckRoll_float_CharacterMaster;
        }

        private bool Util_CheckRoll_float_CharacterMaster(On.RoR2.Util.orig_CheckRoll_float_CharacterMaster orig, float percentChance, CharacterMaster master)
        {
            var result = orig(percentChance, master);
            if (result && master && master.hasBody)
            {
                var body = master.GetBody();
                if (body.HasBuff(buffDef)) body.RemoveBuff(buffDef);
            }
            return result;
        }
    }
}
