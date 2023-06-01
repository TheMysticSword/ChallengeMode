using RoR2;

namespace ChallengeMode.Modifiers
{
    public class EquipmentCDRandomized : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_EQUIPMENTCDRANDOMIZED_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_EQUIPMENTCDRANDOMIZED_DESC";

        public override void OnEnable()
        {
            base.OnEnable();
            On.RoR2.Inventory.DeductEquipmentCharges += Inventory_DeductEquipmentCharges;
        }

        private void Inventory_DeductEquipmentCharges(On.RoR2.Inventory.orig_DeductEquipmentCharges orig, Inventory self, byte slot, int deduction)
        {
            orig(self, slot, deduction);
            if (Util.CheckRoll(33f))
            {
                var equipment = self.GetEquipment(slot);
                var newChargeTime = equipment.chargeFinishTime;
                var rng = RoR2Application.rng.RangeInt(0, 7);
                switch (rng)
                {
                    case 0:
                        newChargeTime = newChargeTime + 7f;
                        break;
                    case 1:
                        newChargeTime = newChargeTime - 7f;
                        break;
                    case 2:
                        newChargeTime = newChargeTime + 15f;
                        break;
                    case 3:
                        newChargeTime = newChargeTime - 15f;
                        break;
                    case 4:
                        newChargeTime = newChargeTime + newChargeTime.timeUntil;
                        break;
                    case 5:
                        newChargeTime = newChargeTime - newChargeTime.timeUntil / 2f;
                        break;
                    case 6:
                        newChargeTime = newChargeTime + 60f;
                        break;
                    case 7:
                        newChargeTime = Run.FixedTimeStamp.now + 135f;
                        break;
                }
                if (newChargeTime.t < 0f)
                    newChargeTime.t = 0f;
                self.SetEquipment(new EquipmentState(equipment.equipmentIndex, newChargeTime, equipment.charges), slot);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.Inventory.DeductEquipmentCharges -= Inventory_DeductEquipmentCharges;
        }
    }
}