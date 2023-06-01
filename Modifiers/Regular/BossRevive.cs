using RoR2;

namespace ChallengeMode.Modifiers
{
    public class BossRevive : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BOSSREVIVE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BOSSREVIVE_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.BossGroup.OnMemberAddedServer += BossGroup_OnMemberAddedServer;
        }

        private void BossGroup_OnMemberAddedServer(On.RoR2.BossGroup.orig_OnMemberAddedServer orig, BossGroup self, CharacterMaster memberMaster)
        {
            orig(self, memberMaster);
            memberMaster.inventory.GiveItem(RoR2Content.Items.ExtraLife);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.BossGroup.OnMemberAddedServer -= BossGroup_OnMemberAddedServer;
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageHasBosses();
        }
    }
}