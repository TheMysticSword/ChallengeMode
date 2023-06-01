using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;

namespace ChallengeMode.CharacterMasters
{
    public class ThunderAttacker : BaseCharacterMaster
    {
        public static CharacterSpawnCard characterSpawnCard;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("ChallengeMode_ThunderAttackerMaster", true);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            masterName = "ChallengeMode_ThunderAttacker";
            Prepare();

            RoR2Application.onLoad += () =>
            {
                prefab.GetComponent<CharacterMaster>().bodyPrefab = BodyCatalog.FindBodyPrefab("MysticsItems_ThunderAttackerBody");
            };

            characterSpawnCard = spawnCard;
        }
    }
}
