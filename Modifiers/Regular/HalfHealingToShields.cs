using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine.Networking;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class HalfHealingToShields : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_HALFHEALINGTOSHIELDS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_HALFHEALINGTOSHIELDS_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal1;
        }

        private void HealthComponent_Heal(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            var healAmountPos = 1;

            if (c.TryGotoNext(
                x => x.MatchMul()
            ) && c.TryGotoNext(
                MoveType.After,
                x => x.MatchStarg(healAmountPos)
            ))
            {
                c.GotoNext(MoveType.AfterLabel);
                c.Emit(OpCodes.Ldarg, 0);
                c.Emit(OpCodes.Ldarg, healAmountPos);
                c.EmitDelegate<System.Func<HealthComponent, float, float>>((hc, healAmount) =>
                {
                    if (hc.body.isPlayerControlled)
                    {
                        healAmount *= 0.5f;
                    }
                    return healAmount;
                });
                c.Emit(OpCodes.Starg, healAmountPos);
            }
            else
            {
                ChallengeModePlugin.logger.LogError("Failed to hook HalfHealingToShields healing modifier");
            }
        }

        private float HealthComponent_Heal1(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            var result = orig(self, amount, procChainMask, nonRegen);
            if (self.body.isPlayerControlled && NetworkServer.active)
            {
                var newShield = Mathf.Min(self.shield + result, self.body.maxShield);
                if (self.shield < newShield)
                    self.Networkshield = newShield;
            }
            return result;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            IL.RoR2.HealthComponent.Heal -= HealthComponent_Heal;
            On.RoR2.HealthComponent.Heal -= HealthComponent_Heal1;
        }

        public override bool IsAvailable()
        {
            return !Buffs.LowHPStress.hookFailed;
        }
    }
}