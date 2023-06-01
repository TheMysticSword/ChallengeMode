using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.CharacterAI;

namespace ChallengeMode.Buffs
{
    public class CommsJammed : BaseBuff
    {
        public override void OnLoad() {
            buffDef.name = "ChallengeMode_CommsJammed";
            buffDef.buffColor = new Color32(203, 214, 213, 255);
            buffDef.canStack = false;
            buffDef.isDebuff = true;
            buffDef.iconSprite = ChallengeModePlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/ChallengeMode/Modifiers/CommsJam/texDebuffCommsJammed.png");

            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            On.RoR2.EquipmentSlot.FixedUpdate += EquipmentSlot_FixedUpdate;
        }

        private void EquipmentSlot_FixedUpdate(On.RoR2.EquipmentSlot.orig_FixedUpdate orig, EquipmentSlot self)
        {
            if (!self.characterBody.HasBuff(buffDef))
            {
                orig(self);
            }
        }

        public void ForEachDroneAI(CharacterBody droneOwnerBody, System.Action<BaseAI> action)
        {
            if (droneOwnerBody.master && droneOwnerBody.master.minionOwnership)
            {
                var minionGroup = MinionOwnership.MinionGroup.FindGroup(droneOwnerBody.master.netId);
                if (minionGroup != null)
                {
                    foreach (var minionOwnership in minionGroup.members)
                    {
                        if (minionOwnership)
                        {
                            var minionMaster = minionOwnership.GetComponent<CharacterMaster>();
                            if (minionMaster && minionMaster.hasBody && minionMaster.GetBody().bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                            {
                                foreach (var aiComponent in minionMaster.aiComponents)
                                {
                                    action(aiComponent);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == this.buffDef && NetworkServer.active)
            {
                self.AddTimedBuff(ChallengeModeContent.Buffs.ChallengeMode_CommsJammedVisuals, 0.2f);
                ForEachDroneAI(self, (aiComponent) =>
                {
                    aiComponent.skillDriverEvaluation = default(BaseAI.SkillDriverEvaluation);
                    aiComponent.skillDriverUpdateTimer = 1000f;
                    aiComponent.selectedSkilldriverName = "";
                    aiComponent.currentEnemy.Reset();
                });
            }
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == this.buffDef && NetworkServer.active)
            {
                ForEachDroneAI(self, (aiComponent) =>
                {
                    aiComponent.skillDriverUpdateTimer = 0f;
                });
            }
        }
    }
}
