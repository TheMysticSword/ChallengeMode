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
    public class ThunderAttacker : BaseCharacterBody
    {
        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("MysticsItems_ThunderAttackerBody", true);
			prefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            Utils.CopyChildren(ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/HotSand/BurnInflictor.prefab"), prefab);
            bodyName = "ChallengeMode_ThunderAttacker";

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
            characterBody.baseMaxHealth = 100000f;
            characterBody.baseDamage = 12f;
            characterBody.baseMoveSpeed = 0f;
            characterBody.baseAcceleration = 0f;
            characterBody.baseJumpPower = 0f;
            characterBody.baseJumpCount = 0;
            characterBody.portraitIcon = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/ShockNearby/bdTeslaField.asset").WaitForCompletion().iconSprite.texture;
			characterBody.bodyColor = new Color32(255, 88, 28, 255);
            characterBody.aimOriginTransform = modelBaseTransform;
            AfterCharacterBodySetup();
            characterBody.baseNameToken = "CHALLENGEMODE_THUNDER_ATTACKER_NAME";
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
}
