using R2API;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class UselessSteak : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_USELESSSTEAK_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_USELESSSTEAK_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                var itemCount = sender.inventory.GetItemCount(RoR2Content.Items.FlatHealth);
                if (itemCount > 0) args.baseHealthAdd -= 24f * itemCount;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
        }
    }
}