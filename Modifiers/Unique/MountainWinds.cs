using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers.Unique
{
    public class MountainWinds : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_MOUNTAINWINDS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_MOUNTAINWINDS_DESC";
        public override bool isAdditional => true;

        public static GameObject windParticlesPrefab;
        public static GameObject windZonePrefab;

        public static float windProjectilePower = 9f;
        public static float windBodyPower = 2f;
        public static float windDropletPower = 8f;
        public static float windPickupPower = 6f;

        public static GameObject windParticlesObject;
        public static GameObject windZoneObject;
        public static List<Component> componentsToDestroyOnDisable = new List<Component>();
        public static WindZone windZone;

        public override void OnEnable()
        {
            base.OnEnable();

            if (!windParticlesPrefab)
                windParticlesPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/MountainWinds/WindParticles.prefab");
            if (!windZonePrefab)
                windZonePrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/MountainWinds/WindZone.prefab");

            if (windParticlesPrefab)
            {
                windParticlesObject = Object.Instantiate(windParticlesPrefab);
                var weatherParticles = windParticlesObject.AddComponent<WeatherParticles>();
                weatherParticles.lockPosition = true;
                weatherParticles.lockRotation = false;
            }

            if (windZonePrefab)
            {
                windZoneObject = Object.Instantiate(windZonePrefab);
                windZone = windZoneObject.GetComponent<WindZone>();
            }

            foreach (var body in CharacterBody.readOnlyInstancesList)
                TryApplyToBody(body);

            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            On.RoR2.Projectile.ProjectileController.Start += ProjectileController_Start;
            On.RoR2.PickupDropletController.Start += PickupDropletController_Start;
            On.RoR2.GenericPickupController.Start += GenericPickupController_Start;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            TryApplyToBody(self);
        }

        public void TryApplyToBody(CharacterBody body)
        {
            if (!body.GetComponent<ChallengeModeWindBodyMover>())
                componentsToDestroyOnDisable.Add(body.gameObject.AddComponent<ChallengeModeWindBodyMover>());
        }

        private void ProjectileController_Start(On.RoR2.Projectile.ProjectileController.orig_Start orig, ProjectileController self)
        {
            orig(self);
            if (!self.GetComponent<ChallengeModeWindProjectileMover>())
                componentsToDestroyOnDisable.Add(self.gameObject.AddComponent<ChallengeModeWindProjectileMover>());
        }

        private void PickupDropletController_Start(On.RoR2.PickupDropletController.orig_Start orig, PickupDropletController self)
        {
            orig(self);
            if (!self.GetComponent<ChallengeModeWindGeneralMover>())
            {
                var mover = self.gameObject.AddComponent<ChallengeModeWindGeneralMover>();
                mover.movePower = windDropletPower;
                componentsToDestroyOnDisable.Add(mover);
            }
        }

        private void GenericPickupController_Start(On.RoR2.GenericPickupController.orig_Start orig, GenericPickupController self)
        {
            orig(self);
            if (!self.GetComponent<ChallengeModeWindGeneralMover>())
            {
                var mover = self.gameObject.AddComponent<ChallengeModeWindGeneralMover>();
                mover.movePower = windPickupPower;
                componentsToDestroyOnDisable.Add(mover);
            }
        }

        public class ChallengeModeWindBodyMover : MonoBehaviour
        {
            public CharacterBody body;
            public IDisplacementReceiver displacementReceiver;
            
            public void Awake()
            {
                body = GetComponent<CharacterBody>();
                displacementReceiver = GetComponent<IDisplacementReceiver>();
            }

            public void FixedUpdate()
            {
                if (body.hasEffectiveAuthority && windZone && displacementReceiver != null)
                {
                    if (!body.characterMotor || !body.characterMotor.disableAirControlUntilCollision)
                    {
                        var windDirection = windZone.transform.forward;
                        var windBodyPowerThisUpdate = windDirection * windBodyPower * Time.fixedDeltaTime;
                        displacementReceiver.AddDisplacement(windBodyPowerThisUpdate);
                    }
                }
            }
        }

        public class ChallengeModeWindProjectileMover : MonoBehaviour
        {
            public ProjectileController projectileController;
            public ProjectileSimple projectileSimple;
            public Rigidbody rigidbody;
            
            public void Awake()
            {
                projectileController = GetComponent<ProjectileController>();
                projectileSimple = GetComponent<ProjectileSimple>();
                if (GetComponent<ConstantForce>() ||
                    GetComponent<Spinner>() ||
                    GetComponentInChildren<CharacterBody>())
                {
                    enabled = false;
                }
            }

            public void FixedUpdate()
            {
                if (windZone && NetworkServer.active)
                {
                    var windDirection = windZone.transform.forward;

                    if (!projectileController.rigidbody || projectileController.rigidbody.velocity.sqrMagnitude > 0)
                    {
                        var projectileTargetDirection = windDirection;
                        projectileTargetDirection.y = transform.forward.y; // don't affect vertical movement
                        transform.forward = Vector3.RotateTowards(transform.forward, projectileTargetDirection, windProjectilePower * 0.0174532924f * Time.fixedDeltaTime, 0f);
                    }

                    if (projectileController.rigidbody && projectileController.rigidbody.velocity.sqrMagnitude > 0 && (!projectileSimple || !projectileSimple.updateAfterFiring || !projectileSimple.enableVelocityOverLifetime))
                    {
                        var horizontalVelocity = projectileController.rigidbody.velocity;
                        horizontalVelocity.y = 0;
                        var horizontalDirection = projectileController.transform.forward;
                        horizontalDirection.y = 0;

                        var verticalVelocity = projectileController.rigidbody.velocity;
                        verticalVelocity.x = 0;
                        verticalVelocity.z = 0;

                        projectileController.rigidbody.velocity = horizontalVelocity.magnitude * horizontalDirection.normalized + verticalVelocity;
                    }
                }
            }
        }

        public class ChallengeModeWindGeneralMover : MonoBehaviour
        {
            public Rigidbody rigidbody;
            public float movePower = 0f;

            public void Awake()
            {
                rigidbody = GetComponent<Rigidbody>();
                if (!rigidbody) enabled = false;
            }

            public void FixedUpdate()
            {
                if (windZone && NetworkServer.active)
                {
                    var windDirection = windZone.transform.forward;
                    var windBodyPowerThisUpdate = windDirection * movePower * Time.fixedDeltaTime;
                    rigidbody.MovePosition(rigidbody.position + windBodyPowerThisUpdate);
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (windParticlesObject) Object.Destroy(windParticlesObject);
            if (windZoneObject) Object.Destroy(windZoneObject);

            foreach (var componentToDestroy in componentsToDestroyOnDisable)
            {
                if (componentToDestroy)
                    Object.Destroy(componentToDestroy);
            }
            componentsToDestroyOnDisable.Clear();

            On.RoR2.CharacterBody.Start -= CharacterBody_Start;
            On.RoR2.Projectile.ProjectileController.Start -= ProjectileController_Start;
            On.RoR2.PickupDropletController.Start -= PickupDropletController_Start;
            On.RoR2.GenericPickupController.Start -= GenericPickupController_Start;
        }

        public override bool IsAvailable()
        {
            return (ChallengeModeUtils.CurrentStageNameMatches("wispgraveyard") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierChance)) ||
                (ChallengeModeUtils.CurrentStageNameMatches("skymeadow") && Util.CheckRoll(ChallengeModeConfig.uniqueModifierRareChance));
        }
    }
}