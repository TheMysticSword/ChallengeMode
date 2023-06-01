using RoR2;
using RoR2.Hologram;
using RoR2.Networking;
using R2API;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;

namespace ChallengeMode.Interactables
{
    public class FrostbiteWarmthTorch : BaseInteractable
    {
        public static DirectorCard directorCard;
        public static DamageColorIndex damageColorIndex;
        public static InteractableSpawnCard interactableSpawnCard;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("MysticsItems_FrostbiteWarmthTorch", true);
            prefab.AddComponent<NetworkTransform>();
            damageColorIndex = DamageColorAPI.RegisterDamageColor(new Color32(153, 255, 246, 255));
        }

        public override void OnLoad()
        {
            base.OnLoad();

            Utils.CopyChildren(ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/Frostbite/FrostbiteWarmthTorch.prefab"), prefab, true);

            modelBaseTransform = prefab.transform.Find("Base");
            modelTransform = prefab.transform.Find("Base/mdlTorch");
            meshObject = prefab.transform.Find("Base/mdlTorch").gameObject;
            genericDisplayNameToken = "CHALLENGEMODE_FROSTBITE_WARMTH_TORCH_NAME";

            Prepare();
            
            var component = prefab.AddComponent<ChallengeModeFrostbiteWarmthTorchBehaviour>();
            component.pulseIndicatorTransform = prefab.transform.Find("Base/mdlTorch/Pulse Sphere");

            spawnCard.hullSize = HullClassification.Human;
            spawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.None;
            spawnCard.directorCreditCost = 0;
            spawnCard.occupyPosition = true;
            spawnCard.orientToFloor = true;
            spawnCard.slightlyRandomizeOrientation = false;
            spawnCard.skipSpawnWhenSacrificeArtifactEnabled = false;

            interactableSpawnCard = spawnCard;

            directorCard = new DirectorCard
            {
                spawnCard = spawnCard,
                selectionWeight = 1,
                spawnDistance = 0f,
                preventOverhead = false,
                minimumStageCompletions = 0,
                requiredUnlockableDef = null,
                forbiddenUnlockableDef = null
            };
        }

        public class ChallengeModeFrostbiteWarmthTorchBehaviour : MonoBehaviour
        {
            public Transform pulseIndicatorTransform;

            public float radius = 14f;
            public float radiusSqr = 196f;
            public float warmthTimer = 0f;
            public float warmthInterval = 0.1f;

            public void Start()
            {
                pulseIndicatorTransform.localScale = Vector3.one * radius;
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    warmthTimer += Time.fixedDeltaTime;
                    if (warmthTimer >= warmthInterval)
                    {
                        warmthTimer -= warmthInterval;
                        foreach (var helper in InstanceTracker.GetInstancesList<Modifiers.Unique.Frostbite.ChallengeModeFrostbiteHelper>())
                        {
                            if (helper.body.isPlayerControlled)
                            {
                                if ((helper.body.corePosition - transform.position).sqrMagnitude <= radiusSqr)
                                {
                                    if (helper.body.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_Frostbite))
                                        helper.body.RemoveBuff(ChallengeModeContent.Buffs.ChallengeMode_Frostbite);
                                    helper.coldTime = 0f;
                                    helper.coldDamageTime = 0f;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}