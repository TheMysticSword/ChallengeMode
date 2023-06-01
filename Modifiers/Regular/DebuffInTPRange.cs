using MysticsRisky2Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeMode.Modifiers
{
    public class DebuffInTPRange : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_DEBUFFINTPRANGE_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_DEBUFFINTPRANGE_DESC";

        public float checkTimer = 0f;
        public float checkInterval = 0.5f;
        public float debuffDuration = 1f;

        public override void OnEnable()
        {
            base.OnEnable();
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private void RoR2Application_onFixedUpdate()
        {
            checkTimer -= Time.fixedDeltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer += checkInterval;

                var allies = TeamComponent.GetTeamMembers(TeamIndex.Player);
                foreach (var holdoutZoneController in InstanceTracker.GetInstancesList<HoldoutZoneController>())
                {
                    if (holdoutZoneController.isActiveAndEnabled)
                    {
                        foreach (var ally in allies)
                        {
                            var body = ally.body;
                            if (body && holdoutZoneController.IsBodyInChargingRadius(body))
                            {
                                body.AddTimedBuff(RoR2Content.Buffs.Slow50, debuffDuration);
                                break;
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