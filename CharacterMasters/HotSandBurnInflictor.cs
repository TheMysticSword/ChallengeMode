using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;

namespace ChallengeMode.CharacterMasters
{
    public class HotSandBurnInflictor : BaseCharacterMaster
    {
        public static CharacterSpawnCard characterSpawnCard;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("ChallengeMode_HotSandBurnInflictorMaster", true);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            masterName = "ChallengeMode_HotSandBurnInflictor";
            Prepare();

            RoR2Application.onLoad += () =>
            {
                prefab.GetComponent<CharacterMaster>().bodyPrefab = BodyCatalog.FindBodyPrefab("MysticsItems_HotSandBurnInflictorBody");
            };

            characterSpawnCard = spawnCard;
        }
    }
}
