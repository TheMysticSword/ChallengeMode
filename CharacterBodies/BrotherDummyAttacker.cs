using UnityEngine;
using MysticsRisky2Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2.Navigation;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.CharacterBodies
{
    public class BrotherDummyAttacker : BaseCharacterBody
    {
        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("MysticsItems_BrotherDummyAttackerBody", true);
			prefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            Utils.CopyChildren(ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/HotSand/BurnInflictor.prefab"), prefab);
            bodyName = "ChallengeMode_BrotherDummyAttacker";

            modelBaseTransform = prefab.transform.Find("ModelBase");
            modelTransform = prefab.transform.Find("ModelBase/EmptyMesh");
            meshObject = modelTransform.gameObject;
            Prepare();

            SetUpChildLocator(new ChildLocator.NameTransformPair[0]);

            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = false;
            modelLocator.dontDetatchFromParent = true;
            modelLocator.noCorpse = true;

            // body
            CharacterBody characterBody = prefab.GetComponent<CharacterBody>();
            characterBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToGoo | CharacterBody.BodyFlags.ImmuneToExecutes | CharacterBody.BodyFlags.ImmuneToVoidDeath | CharacterBody.BodyFlags.HasBackstabImmunity;
            characterBody.baseMaxHealth = 200f;
            characterBody.baseDamage = 18f;
            characterBody.baseMoveSpeed = 0f;
            characterBody.baseAcceleration = 0f;
            characterBody.baseJumpPower = 0f;
            characterBody.baseJumpCount = 0;
            characterBody.portraitIcon = Addressables.LoadAssetAsync<Texture2D>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png").WaitForCompletion();
			characterBody.bodyColor = Color.black;
            characterBody.aimOriginTransform = modelBaseTransform;
            AfterCharacterBodySetup();
            characterBody.baseNameToken = "UNIDENTIFIED";
            characterBody.subtitleNameToken = "";

            prefab.AddComponent<ChallengeModeRejectAllDamage>();

            // state machines
            EntityStateMachine bodyStateMachine = SetUpEntityStateMachine("Body", typeof(EntityStates.Uninitialized), typeof(EntityStates.Uninitialized));
            
            CharacterDeathBehavior characterDeathBehavior = prefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = bodyStateMachine;
            characterDeathBehavior.deathState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Turret1.DeathState));

            // model
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[0];
			AfterCharacterModelSetUp();
        }
    }

    public class ChallengeModeRejectAllDamage : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            damageInfo.rejected = true;
        }
    }
}
