using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.Audio;
using UnityEngine.Networking;

namespace ChallengeMode.Buffs
{
    public class SuperArmor : BaseBuff
    {
        public static NetworkSoundEventDef shellPrepSound;
        
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_SuperArmor";
            buffDef.buffColor = new Color32(48, 165, 255, 255);
            buffDef.canStack = false;
            buffDef.isDebuff = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/LunarGolem/texBuffLunarShellIcon.tif").WaitForCompletion();

            Overlays.CreateOverlay(Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarGolem/matLunarGolemShield.mat").WaitForCompletion(), (model) =>
            {
                return model.body && model.body.HasBuff(buffDef);
            });

            shellPrepSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            shellPrepSound.eventName = "Play_lunar_golem_attack2_buildUp";
            ChallengeModeContent.Resources.networkSoundEventDefs.Add(shellPrepSound);

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);

            if (NetworkServer.active && buffDef == this.buffDef)
            {
                var effectData = new EffectData
                {
                    origin = self.corePosition,
                    rotation = Quaternion.Euler(self.coreTransform.forward)
                };
                effectData.SetHurtBoxReference(self.mainHurtBox);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/LunarGolemShieldCharge"), effectData, true);
                EntitySoundManager.EmitSoundServer(shellPrepSound.index, self.networkIdentity);
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(buffDef))
            {
                args.armorAdd += 500f;
            }
        }
    }
}
