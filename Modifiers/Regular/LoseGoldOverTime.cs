using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class LoseGoldOverTime : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_LOSEGOLDOVERTIME_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_LOSEGOLDOVERTIME_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            foreach (var body in CharacterBody.readOnlyInstancesList)
            {
                if (body.isPlayerControlled && body.master && !body.master.GetComponent<ChallengeModeLoseGoldOverTime>())
                    body.master.gameObject.AddComponent<ChallengeModeLoseGoldOverTime>();
            }
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.isPlayerControlled && self.master && !self.master.GetComponent<ChallengeModeLoseGoldOverTime>())
                self.master.gameObject.AddComponent<ChallengeModeLoseGoldOverTime>();
        }

        public class ChallengeModeLoseGoldOverTime : MonoBehaviour
        {
            public CharacterMaster master;
            public uint goldLossPerTick = 1u;
            public float tickInterval = 3.5f;
            public float tickTimer = 0f;

            public void Awake()
            {
                master = GetComponent<CharacterMaster>();
                if (Run.instance)
                {
                    goldLossPerTick = (uint)Run.instance.GetDifficultyScaledCost(1);
                }
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    tickTimer += Time.fixedDeltaTime;
                    if (tickTimer >= tickInterval)
                    {
                        tickTimer -= tickInterval;
                        master.money -= System.Math.Min(goldLossPerTick, master.money);
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            foreach (var master in CharacterMaster.readOnlyInstancesList)
            {
                var component = master.GetComponent<ChallengeModeLoseGoldOverTime>();
                if (component)
                    Object.Destroy(component);
            }
        }
    }
}