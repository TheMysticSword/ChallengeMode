using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using R2API;

namespace ChallengeMode.Items
{
    public class PermanentImmuneToVoidDeath : BaseItem
    {
        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "ChallengeMode_PermanentImmuneToVoidDeath";
            SetItemTierWhenAvailable(ItemTier.NoTier);
            itemDef.canRemove = false;
            itemDef.tags = new ItemTag[]
            {
                ItemTag.AIBlacklist,
                ItemTag.CannotCopy
            };

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCount(itemDef) > 0)
            {
                if (!sender.bodyFlags.HasFlag(CharacterBody.BodyFlags.ImmuneToVoidDeath))
                    sender.bodyFlags |= CharacterBody.BodyFlags.ImmuneToVoidDeath;
            }
        }
    }
}
