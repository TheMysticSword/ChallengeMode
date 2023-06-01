using MysticsRisky2Utils;
using RoR2;
using R2API;

namespace ChallengeMode.Modifiers
{
    public class BlocksForHealth : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BLOCKSFORHEALTH_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BLOCKSFORHEALTH_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (body.teamComponent && body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    for (var i = body.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_BlockNextDamage); i < 10; i++)
                    {
                        body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_BlockNextDamage);
                    }
                }
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.teamComponent.teamIndex == TeamIndex.Player)
            {
                args.baseCurseAdd += 0.1f;
            }
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.teamComponent && self.teamComponent.teamIndex == TeamIndex.Player)
            {
                for (var i = self.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_BlockNextDamage); i < 10; i++)
                {
                    self.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_BlockNextDamage);
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (body.teamComponent && body.teamComponent.teamIndex == TeamIndex.Player)
                {
                    for (var i = 0; i < body.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_BlockNextDamage); i++)
                    {
                        body.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_BlockNextDamage);
                    }
                }
            }
        }
    }
}