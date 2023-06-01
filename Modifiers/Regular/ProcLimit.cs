using UnityEngine.Networking;
using RoR2;
using UnityEngine;
using System.Linq;
using R2API;

namespace ChallengeMode.Modifiers
{
    public class ProcLimit : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_PROCLIMIT_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_PROCLIMIT_DESC";

        public int procLimit = 999;
        public int luckChange = -1;
        
        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.isPlayerControlled)
            {
                for (var i = 0; i < procLimit; i++)
                {
                    self.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator);
                }
            }
        }

        public void ModifyMasterLuck(CharacterMaster master)
        {
            if (master.playerCharacterMasterController && master.hasBody)
            {
                var body = master.GetBody();
                if (!body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator))
                {
                    master.luck += luckChange;
                }
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.isPlayerControlled && !self.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator))
            {
                ModifyMasterLuck(self.master);
            }
        }

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            ModifyMasterLuck(self);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
        }
    }
}