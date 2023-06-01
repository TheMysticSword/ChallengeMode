using R2API;
using RoR2;

namespace ChallengeMode.Modifiers
{
    public class FireBackfire : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_FIREBACKFIRE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_FIREBACKFIRE_DESC";

        public static float triggerChance = 20f;
        public static float damageMultiplier = 0.5f;

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.DotController.OnDotStackAddedServer += DotController_OnDotStackAddedServer;
        }

        private void DotController_OnDotStackAddedServer(On.RoR2.DotController.orig_OnDotStackAddedServer orig, DotController self, object dotStack)
        {
            orig(self, dotStack);
            var dotStackCast = (DotController.DotStack)dotStack;
            if (dotStackCast.dotIndex == DotController.DotIndex.Burn || dotStackCast.dotIndex == DotController.DotIndex.StrongerBurn)
            {
                if (self.victimObject != dotStackCast.attackerObject && self.victimBody && self.victimBody.teamComponent && self.victimBody.teamComponent.teamIndex != dotStackCast.attackerTeam && dotStackCast.attackerTeam == TeamIndex.Player)
                {
                    if (self.victimHealthComponent && self.victimHealthComponent.alive)
                    {
                        if (Util.CheckRoll(triggerChance))
                        {
                            var dotDef = dotStackCast.dotDef;
                            var inflictDotInfo = new InflictDotInfo
                            {
                                victimObject = dotStackCast.attackerObject,
                                attackerObject = dotStackCast.attackerObject,
                                totalDamage = new float?(dotStackCast.damage * (dotStackCast.timer / dotDef.interval) * damageMultiplier),
                                dotIndex = DotController.DotIndex.Burn,
                                damageMultiplier = 1f
                            };
                            if (self.victimBody.inventory)
                                StrengthenBurnUtils.CheckDotForUpgrade(self.victimBody.inventory, ref inflictDotInfo);
                            DotController.InflictDot(ref inflictDotInfo);
                        }
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.DotController.OnDotStackAddedServer -= DotController_OnDotStackAddedServer;
        }
    }
}