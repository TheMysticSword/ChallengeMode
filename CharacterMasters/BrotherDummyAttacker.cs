using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;

namespace ChallengeMode.CharacterMasters
{
    public class BrotherDummyAttacker : BaseCharacterMaster
    {
        public static CharacterSpawnCard characterSpawnCard;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("ChallengeMode_BrotherDummyAttackerMaster", true);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            masterName = "ChallengeMode_BrotherDummyAttacker";
            Prepare();

            RoR2Application.onLoad += () =>
            {
                prefab.GetComponent<CharacterMaster>().bodyPrefab = BodyCatalog.FindBodyPrefab("MysticsItems_BrotherDummyAttackerBody");
            };

            characterSpawnCard = spawnCard;
        }
    }
}
