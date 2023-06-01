using RoR2;
using RoR2.Artifacts;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class DronePurgatory : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_DRONEPURGATORY_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_DRONEPURGATORY_DESC";

        public int phase = 0;

        public override void OnEnable()
        {
            base.OnEnable();
            phase = CalculateCurrentPhase();
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        public int CalculateCurrentPhase()
        {
            if (Run.instance)
            {
                var stopwatch = Run.instance.GetRunStopwatch();
                return Mathf.FloorToInt(stopwatch / 60f);
            }
            return 0;
        }

        public bool UpdatePhase()
        {
            if (Run.instance)
            {
                var newPhase = CalculateCurrentPhase();
                if (phase < newPhase)
                {
                    phase = newPhase;
                    return true;
                }
            }
            return false;
        }

        private void RoR2Application_onFixedUpdate()
        {
            if (NetworkServer.active && UpdatePhase())
            {
                var drones = CharacterBody.readOnlyInstancesList
                    .Where(
                        x => x.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical) &&
                        x.teamComponent.teamIndex == TeamIndex.Player &&
                        x.healthComponent &&
                        x.healthComponent.alive &&
                        x.master &&
                        x.master.minionOwnership.ownerMaster
                    ).ToList();
                if (drones.Count > 0)
                {
                    var drone = RoR2Application.rng.NextElementUniform(drones);
                    drone.healthComponent.Suicide();
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RoR2Application.onFixedUpdate -= RoR2Application_onFixedUpdate;
        }

        public override bool IsAvailable()
        {
            return Stage.instance && Stage.instance.sceneDef && Stage.instance.sceneDef.sceneType == SceneType.Stage;
        }
    }
}