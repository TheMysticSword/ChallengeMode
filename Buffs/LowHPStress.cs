using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace ChallengeMode.Buffs
{
    public class LowHPStress : BaseBuff
    {
        public static bool hookFailed = false;

        public override void OnLoad() {
            buffDef.name = "ChallengeMode_LowHPStress";
            buffDef.buffColor = new Color32(102, 2, 5, 255);
            buffDef.canStack = false;
            buffDef.isDebuff = true;
            buffDef.isHidden = false;
            buffDef.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Croco/bdCrocoRegen.asset").WaitForCompletion().iconSprite;

            IL.RoR2.HealthComponent.Heal += HealthComponent_Heal;
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
                    if (hc.body.HasBuff(buffDef))
                    {
                        healAmount *= 0.5f;
                    }
                    return healAmount;
                });
                c.Emit(OpCodes.Starg, healAmountPos);

                hookFailed = false;
            }
            else
            {
                if (!hookFailed)
                {
                    ChallengeModePlugin.logger.LogError("Failed to hook LowHPStress healing modifier");
                    hookFailed = true;
                }
            }
        }
    }
}
