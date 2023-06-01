using MonoMod.RuntimeDetour;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class NoGoingBack : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_NOGOINGBACK_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_NOGOINGBACK_DESC";

        public Hook hook;

        public override void OnEnable()
        {
            base.OnEnable();
            hook = new Hook(typeof(Rewired.Player).GetMethod("GetAxis", new [] {typeof(int)}), new System.Func<System.Func<Rewired.Player, int, float>, Rewired.Player, int, float>((orig, self, actionId) =>
            {
                var result = orig(self, actionId);
                if (actionId == 1)
                {
                    result = Mathf.Max(result, 0);
                }
                return result;
            }));
        }

        public override void OnDisable()
        {
            base.OnDisable();
            hook.Free();
        }
    }
}