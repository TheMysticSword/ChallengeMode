using RoR2;
using RoR2.Artifacts;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class BombDrop : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_BOMBDROP_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_BOMBDROP_DESC";

        public float timer = 0f;
        public float interval = 5f;

        public override void OnEnable()
        {
            base.OnEnable();
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private void RoR2Application_onFixedUpdate()
        {
            if (NetworkServer.active)
            {
                timer += Time.fixedDeltaTime;
                if (timer >= interval)
                {
                    timer -= interval;
                    foreach (var body in CharacterBody.readOnlyInstancesList)
                    {
                        if (body.isPlayerControlled)
                        {
                            var bombSpawnPosition = body.corePosition;
                            var downRay = new Ray(bombSpawnPosition + Vector3.up * BombArtifactManager.maxBombStepUpDistance, Vector3.down);
                            if (Physics.Raycast(downRay, out var raycastHit, BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                            {
                                var groundY = raycastHit.point.y;
                                if (bombSpawnPosition.y < groundY + 4f)
                                {
                                    bombSpawnPosition.y = groundY + 4f;
                                }

                                var bomb = Object.Instantiate(BombArtifactManager.bombPrefab, bombSpawnPosition, Random.rotation);
                                var spiteBombController = bomb.GetComponent<SpiteBombController>();
                                var delayBlast = spiteBombController.delayBlast;
                                var teamFilter = bomb.GetComponent<TeamFilter>();
                                spiteBombController.bouncePosition = raycastHit.point;
                                spiteBombController.initialVelocityY = 8f;
                                delayBlast.position = body.corePosition;
                                delayBlast.baseDamage = body.damage * 3f;
                                delayBlast.baseForce = 2300f;
                                delayBlast.attacker = body.gameObject;
                                delayBlast.radius = 13f;
                                delayBlast.crit = false;
                                delayBlast.procCoefficient = 0.75f;
                                delayBlast.maxTimer = BombArtifactManager.bombFuseTimeout;
                                delayBlast.timerStagger = 0f;
                                delayBlast.falloffModel = BlastAttack.FalloffModel.None;
                                teamFilter.teamIndex = TeamIndex.Monster;
                                NetworkServer.Spawn(bomb);
                            }
                        }
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            RoR2Application.onFixedUpdate -= RoR2Application_onFixedUpdate;
        }
    }
}