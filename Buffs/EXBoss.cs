using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using MysticsRisky2Utils;
using R2API;

namespace ChallengeMode.Buffs
{
    public class EXBoss : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_EXBoss";
            buffDef.buffColor = Color.white;
            buffDef.canStack = false;
            buffDef.isDebuff = false;
            buffDef.isHidden = true;

            Overlays.CreateOverlay(ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/EXBoss/matEXBossOverlay.mat"), (characterModel) =>
            {
                return characterModel.body && characterModel.body.radius < 5f && characterModel.body.HasBuff(buffDef);
            });
            Overlays.CreateOverlay(ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/EXBoss/matEXBossOverlayGiant.mat"), (characterModel) =>
            {
                return characterModel.body && characterModel.body.radius >= 5f && characterModel.body.HasBuff(buffDef);
            });

            On.RoR2.Util.GetBestBodyName += Util_GetBestBodyName;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            GenericGameEvents.OnApplyDamageReductionModifiers += GenericGameEvents_OnApplyDamageReductionModifiers;
        }

        private string Util_GetBestBodyName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var result = orig(bodyObject);
            if (bodyObject)
            {
                var body = bodyObject.GetComponent<CharacterBody>();
                if (body && body.HasBuff(buffDef))
                {
                    result = Language.GetStringFormatted("BODY_MODIFIER_CHALLENGEMODE_EXBOSS", result);
                }
            }
            return result;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                args.moveSpeedMultAdd += 0.2f;
            }
        }

        private void GenericGameEvents_OnApplyDamageReductionModifiers(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo, ref float damage)
        {
            if (attackerInfo.body && attackerInfo.body.HasBuff(buffDef))
                damage *= 0.7f;
        }
    }
}
