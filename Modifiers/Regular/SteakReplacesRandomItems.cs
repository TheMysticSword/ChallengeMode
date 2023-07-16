using R2API;
using RoR2;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class SteakReplacesRandomItems : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_STEAKREPLACESRANDOMITEMS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_STEAKREPLACESRANDOMITEMS_DESC";

        public float chance = 20f;
        public PickupIndex steakPickupIndex;

        public override void OnEnable()
        {
            base.OnEnable();
            steakPickupIndex = PickupCatalog.FindPickupIndex(RoR2Content.Items.FlatHealth.itemIndex);
            On.RoR2.PickupDropletController.OnCollisionEnter += PickupDropletController_OnCollisionEnter;
        }

        private void PickupDropletController_OnCollisionEnter(On.RoR2.PickupDropletController.orig_OnCollisionEnter orig, PickupDropletController self, UnityEngine.Collision collision)
        {
            if (NetworkServer.active && Util.CheckRoll(chance))
            {
                self.createPickupInfo.pickerOptions = PickupPickerController.GenerateOptionsFromArray(new PickupIndex[] { steakPickupIndex });
				self.createPickupInfo.pickupIndex = steakPickupIndex;
                self.NetworkpickupIndex = steakPickupIndex;
            }
            orig(self, collision);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.PickupDropletController.OnCollisionEnter -= PickupDropletController_OnCollisionEnter;
        }
    }
}