using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Buffs
{
    public class Disarmed : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_Disarmed";
            buffDef.buffColor = new Color32(247, 61, 77, 255);
            buffDef.canStack = false;
            buffDef.isDebuff = true;
            buffDef.isHidden = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Nullifier/texBuffNullifiedIcon.tif").WaitForCompletion();

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += CharacterBody_UpdateAllTemporaryVisualEffects;
            On.RoR2.Run.AdvanceStage += Run_AdvanceStage;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                if (sender.skillLocator.primary) sender.skillLocator.primary.SetSkillOverride(sender, VoidRaidCrabEXAssets.skillDisarmed, GenericSkill.SkillOverridePriority.Contextual);
            }
            else
            {
                if (sender.skillLocator.primary) sender.skillLocator.primary.UnsetSkillOverride(sender, VoidRaidCrabEXAssets.skillDisarmed, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(buffDef)) self.maxJumpCount -= 9999;
        }

        private void CharacterBody_UpdateAllTemporaryVisualEffects(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self);
            if (!tempVisualEffectPerBody.ContainsKey(self))
                tempVisualEffectPerBody[self] = null;
            var tempVisualEffect = tempVisualEffectPerBody[self];
            self.UpdateSingleTemporaryVisualEffect(ref tempVisualEffect, CharacterBody.AssetReferences.nullifyStack1EffectPrefab, self.radius, self.HasBuff(buffDef));
            tempVisualEffectPerBody[self] = tempVisualEffect;
        }

        private void Run_AdvanceStage(On.RoR2.Run.orig_AdvanceStage orig, Run self, SceneDef nextScene)
        {
            orig(self, nextScene);
            tempVisualEffectPerBody.Clear();
        }

        public static Dictionary<CharacterBody, TemporaryVisualEffect> tempVisualEffectPerBody = new Dictionary<CharacterBody, TemporaryVisualEffect>();
    }
}
