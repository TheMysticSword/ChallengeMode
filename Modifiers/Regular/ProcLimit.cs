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
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            On.RoR2.Util.CheckRoll_float_CharacterMaster += Util_CheckRoll_float_CharacterMaster;
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

        private void CharacterMaster_OnInventoryChanged(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);
            if (self.playerCharacterMasterController && self.hasBody)
            {
                var body = self.GetBody();
                if (!body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator))
                {
                    self.luck += luckChange;
                }
            }
        }

        private bool Util_CheckRoll_float_CharacterMaster(On.RoR2.Util.orig_CheckRoll_float_CharacterMaster orig, float percentChance, CharacterMaster master)
        {
            var result = orig(percentChance, master);
            if (result && master && master.hasBody)
            {
                var body = master.GetBody();
                if (body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator))
                {
                    body.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator);
                    if (!body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_ProcLimitIndicator))
                    {
                        master.luck += luckChange;
                    }
                }
            }
            return result;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            On.RoR2.CharacterMaster.OnInventoryChanged -= CharacterMaster_OnInventoryChanged;
            On.RoR2.Util.CheckRoll_float_CharacterMaster -= Util_CheckRoll_float_CharacterMaster;
        }
    }
}