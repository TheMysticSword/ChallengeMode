using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.Modifiers
{
    public class DoubleJumpHurts : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_DOUBLEJUMPHURTS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_DOUBLEJUMPHURTS_DESC";

		public static GameObject effectPrefab;

        public override void OnEnable()
        {
            base.OnEnable();

			if (!effectPrefab)
            {
                effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Imp/SpurtImpBlood.prefab").WaitForCompletion();
            }

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.EntityStates.GenericCharacterMain.ProcessJump += GenericCharacterMain_ProcessJump;
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.teamComponent.teamIndex == TeamIndex.Player) self.maxJumpCount += 1;
        }

        private void GenericCharacterMain_ProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self)
        {
            var oldJumpCount = 0;
            if (self.characterMotor) oldJumpCount = self.characterMotor.jumpCount;
            
            orig(self);

            if (self.hasCharacterMotor)
            {
                if (self.jumpInputReceived && self.characterBody && self.teamComponent && self.teamComponent.teamIndex == TeamIndex.Player && self.characterMotor.jumpCount == self.characterBody.baseJumpCount + 1 && oldJumpCount < self.characterMotor.jumpCount && self.healthComponent)
                {
                    var damageInfo = new DamageInfo();
                    damageInfo.damage = self.characterBody.damage * 0.8f;
                    damageInfo.attacker = self.characterBody.gameObject;
                    damageInfo.inflictor = self.gameObject;
                    damageInfo.force = Vector3.zero;
                    damageInfo.crit = self.characterBody.RollCrit();
                    damageInfo.procCoefficient = 1f;
                    damageInfo.position = self.characterBody.footPosition;
                    damageInfo.damageColorIndex = DamageColorIndex.Bleed;
                    damageInfo.damageType = DamageType.BypassBlock;

                    var couldReceiveBackstab = self.characterBody.canReceiveBackstab;
                    if (couldReceiveBackstab)
                        self.characterBody.canReceiveBackstab = false; // prevent bandit from backstabbing himself with this
                    self.healthComponent.TakeDamage(damageInfo);
                    self.characterBody.canReceiveBackstab = couldReceiveBackstab;

                    GlobalEventManager.instance.OnHitEnemy(damageInfo, self.healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, self.healthComponent.gameObject);

                    if (effectPrefab)
                    {
                        EffectManager.SpawnEffect(effectPrefab, new EffectData
                        {
                            origin = self.characterBody.footPosition
                        }, true);
                    }
                }
            }
		}

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CharacterBody.RecalculateStats -= CharacterBody_RecalculateStats;
            On.EntityStates.GenericCharacterMain.ProcessJump -= GenericCharacterMain_ProcessJump;
        }
    }
}