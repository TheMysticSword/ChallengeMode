using RoR2;
using RoR2.Artifacts;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class RandomMeteors : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_RANDOMMETEORS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_RANDOMMETEORS_DESC";

        public float timer = 0f;
        public float intervalMin = 90f;
        public float intervalMax = 150f;

        public override void OnEnable()
        {
            base.OnEnable();
            timer = RoR2Application.rng.RangeFloat(30f, 60f);
            if (NetworkServer.active)
            {
                RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
            }
        }

        private void RoR2Application_onFixedUpdate()
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0f)
            {
                timer += RoR2Application.rng.RangeFloat(intervalMin, intervalMax);

                var controller = Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/MeteorStorm"), Vector3.zero, Quaternion.identity).GetComponent<MeteorStormController>();
                controller.owner = null;
                controller.ownerDamage = 12f * (1f + 0.2f * (Run.instance.ambientLevel - 1f));
                controller.isCrit = false;
                NetworkServer.Spawn(controller.gameObject);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RoR2Application.onFixedUpdate -= RoR2Application_onFixedUpdate;
        }
    }
}