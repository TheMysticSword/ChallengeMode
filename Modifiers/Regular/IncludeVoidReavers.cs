using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode.Modifiers
{
    public class IncludeVoidReavers : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_INCLUDEVOIDREAVERS_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_INCLUDEVOIDREAVERS_DESC";

        public CharacterSpawnCard spawnCard;

        public override void OnEnable()
        {
            base.OnEnable();

            if (!spawnCard)
                spawnCard = Addressables.LoadAssetAsync<CharacterSpawnCard>("RoR2/Base/Nullifier/cscNullifier.asset").WaitForCompletion();

            On.RoR2.CombatDirector.AttemptSpawnOnTarget += CombatDirector_AttemptSpawnOnTarget;
        }

        private bool CombatDirector_AttemptSpawnOnTarget(On.RoR2.CombatDirector.orig_AttemptSpawnOnTarget orig, CombatDirector self, Transform spawnTarget, DirectorPlacementRule.PlacementMode placementMode)
        {
            if (!self.GetComponent<ChallengeModeVoidReaversIncluded>())
            {
                self.gameObject.AddComponent<ChallengeModeVoidReaversIncluded>();
                self.finalMonsterCardsSelection.AddChoice(new DirectorCard
                {
                    spawnCard = spawnCard,
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    selectionWeight = 1
                }, 1f);
            }
            return orig(self, spawnTarget, placementMode);
        }

        public class ChallengeModeVoidReaversIncluded : MonoBehaviour { }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.CombatDirector.AttemptSpawnOnTarget -= CombatDirector_AttemptSpawnOnTarget;
        }

        public override bool IsAvailable()
        {
            return Run.instance && Run.instance.stageClearCount >= 2;
        }
    }
}