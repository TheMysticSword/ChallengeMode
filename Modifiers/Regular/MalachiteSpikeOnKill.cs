using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class MalachiteSpikeOnKill : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_MALACHITESPIKEONKILL_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_MALACHITESPIKEONKILL_DESC";

        public GameObject orbPrefab;

        public override void OnEnable()
        {
            base.OnEnable();
            if (!orbPrefab) orbPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/PoisonOrbProjectile");
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.victimTeamIndex != TeamIndex.Player && damageReport.victimTeamIndex != TeamIndex.Neutral && damageReport.victimBody)
            {
                var orbCount = 1 + (int)damageReport.victimBody.radius;

                var randomRotation = RoR2Application.rng.RangeFloat(0f, 360f);
                var rotationSlice = 360f / (float)orbCount;
                var forwardNormalized = Vector3.ProjectOnPlane(damageReport.victimBody.transform.forward, Vector3.up).normalized;
                var rotateForward = Vector3.RotateTowards(Vector3.up, forwardNormalized, 0.436332315f, float.PositiveInfinity);
                for (var i = 0; i < orbCount; i++)
                {
                    var orbDirection = Vector3.up;
                    if (orbCount > 1)
                        orbDirection = Quaternion.AngleAxis(randomRotation + rotationSlice * (float)i, Vector3.up) * rotateForward;
                    ProjectileManager.instance.FireProjectile(
                        orbPrefab,
                        damageReport.victimBody.corePosition,
                        Util.QuaternionSafeLookRotation(orbDirection),
                        damageReport.victimBody.gameObject,
                        damageReport.victimBody.damage * 1f,
                        0f,
                        damageReport.victimBody.RollCrit()
                    );
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
        }
    }
}