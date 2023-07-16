using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class DroppingItems : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_DROPPINGITEMS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_DROPPINGITEMS_DESC";

        public float timer = 0.5f;
        public float intervalMin = 5f;
        public float intervalMax = 30f;

        public override void OnEnable()
        {
            base.OnEnable();
            timer = RoR2Application.rng.RangeFloat(15f, 30f);
            RoR2Application.onFixedUpdate += RoR2Application_onFixedUpdate;
        }

        private void RoR2Application_onFixedUpdate()
        {
            if (NetworkServer.active)
            {
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                {
                    timer += RoR2Application.rng.RangeFloat(intervalMin, intervalMax);
                    foreach (var body in CharacterBody.readOnlyInstancesList)
                    {
                        if (body.isPlayerControlled && body.inventory)
                        {
                            var removableItems = body.inventory.itemAcquisitionOrder
                                .Select(x => ItemCatalog.GetItemDef(x))
                                .Where(x => x.canRemove && !x.hidden).ToList();
                            if (removableItems.Count > 0)
                            {
                                var removedItem = RoR2Application.rng.NextElementUniform(removableItems);
                                body.inventory.RemoveItem(removedItem);
                                var randomCirclePoint = Random.insideUnitCircle.normalized;
                                PickupDropletController.CreatePickupDroplet(
                                    PickupCatalog.FindPickupIndex(removedItem.itemIndex),
                                    body.corePosition + Vector3.up * 1.5f,
                                    Vector3.up * 10f + new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y) * 4f
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
    }
}