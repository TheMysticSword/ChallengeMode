using R2API;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class BackpackWeight : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BACKPACKWEIGHT_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BACKPACKWEIGHT_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                TryUpdateBody(body);
            }
        }

        public void TryUpdateBody(CharacterBody body)
        {
            if (body.inventory && body.teamComponent.teamIndex == TeamIndex.Player)
            {
                var totalItemCount = 0;
                var itemIndex = (ItemIndex)0;
                while (itemIndex < (ItemIndex)ItemCatalog.itemCount)
                {
                    var itemDef = ItemCatalog.GetItemDef(itemIndex);
                    if (!itemDef.hidden)
                    {
                        totalItemCount += body.inventory.GetItemCount(itemIndex);
                    }
                    itemIndex++;
                }

                var buffCount = body.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_BackpackWeight);
                if (buffCount > totalItemCount)
                {
                    for (var i = 0; i < buffCount - totalItemCount; i++)
                    {
                        body.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_BackpackWeight);
                    }
                }
                else if (buffCount < totalItemCount)
                {
                    for (var i = 0; i < totalItemCount - buffCount; i++)
                    {
                        body.AddBuff(ChallengeModeContent.Buffs.ChallengeMode_BackpackWeight);
                    }
                }
            }
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            TryUpdateBody(self);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.OnInventoryChanged -= CharacterBody_OnInventoryChanged;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                var buffCount = body.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_BackpackWeight);
                for (var i = 0; i < buffCount; i++)
                    body.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_BackpackWeight);
            }
        }
    }
}