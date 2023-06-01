using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class RerollItems : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_REROLLITEMS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_REROLLITEMS_DESC";

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
                foreach (var body in CharacterBody.readOnlyInstancesList)
                {
                    if (body.isPlayerControlled && body.inventory)
                    {
                        var rerollableItems = body.inventory.itemAcquisitionOrder
                            .Select(x => ItemCatalog.GetItemDef(x))
                            .Where(x => x.canRemove && !x.hidden && x.DoesNotContainTag(ItemTag.WorldUnique)).ToList();
                        if (rerollableItems.Count > 0)
                        {
                            var rerolledItem = RoR2Application.rng.NextElementUniform(rerollableItems);
                            var itemCount = body.inventory.GetItemCount(rerolledItem);
                            var rerolledPickupIndex = PickupCatalog.FindPickupIndex(rerolledItem.itemIndex);
                            var rerollOptions = PickupTransmutationManager.GetAvailableGroupFromPickupIndex(rerolledPickupIndex)
                                .Where(x => x != rerolledPickupIndex)
                                .ToList();
                            if (rerollOptions != null && rerollOptions.Count > 0)
                            {
                                var newItem = PickupCatalog.GetPickupDef(RoR2Application.rng.NextElementUniform(rerollOptions)).itemIndex;
                                body.inventory.RemoveItem(rerolledItem, itemCount);
                                body.inventory.GiveItem(newItem, itemCount);
                                CharacterMasterNotificationQueue.SendTransformNotification(
                                    body.master,
                                    rerolledItem.itemIndex,
                                    newItem,
                                    CharacterMasterNotificationQueue.TransformationType.Default
                                );
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

        public override bool IsAvailable()
        {
            return Stage.instance && Stage.instance.sceneDef && Stage.instance.sceneDef.sceneType == SceneType.Stage;
        }
    }
}