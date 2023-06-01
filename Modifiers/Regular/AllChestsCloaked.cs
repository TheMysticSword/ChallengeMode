using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ChallengeMode.Modifiers
{
    public class AllChestsCloaked : ChallengeRunModifier
    {
        public override string nameToken => "CHALLENGEMODE_MODIFIER_ALLCHESTSCLOAKED_NAME";
        public override string descriptionToken => "CHALLENGEMODE_MODIFIER_ALLCHESTSCLOAKED_DESC";

        public static Material cloakedMaterial;

        public override void OnEnable()
        {
            base.OnEnable();

            if (!cloakedMaterial)
                cloakedMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/matCloakedEffect.mat").WaitForCompletion();

            On.RoR2.PurchaseInteraction.Awake += PurchaseInteraction_Awake;
            foreach (var purchaseInteraction in InstanceTracker.GetInstancesList<PurchaseInteraction>())
            {
                TryToApply(purchaseInteraction);
            }
        }

        public static void TryToApply(PurchaseInteraction purchaseInteraction)
        {
            if (purchaseInteraction.displayNameToken.Contains("CHEST") &&
                !purchaseInteraction.displayNameToken.Contains("VOID") &&
                !purchaseInteraction.displayNameToken.Contains("LUNAR") &&
                purchaseInteraction.GetComponent<ChestBehavior>() &&
                !purchaseInteraction.GetComponent<ChallengeModeCloakedChest>())
            {
                purchaseInteraction.gameObject.AddComponent<ChallengeModeCloakedChest>();
            }
        }

        private void PurchaseInteraction_Awake(On.RoR2.PurchaseInteraction.orig_Awake orig, PurchaseInteraction self)
        {
            orig(self);
            TryToApply(self);
        }

        public class ChallengeModeCloakedChest : MonoBehaviour
        {
            public ModelLocator modelLocator;

            public Dictionary<Renderer, Material[]> materialsPerRenderer = new Dictionary<Renderer, Material[]>();

            public RoR2.Hologram.HologramProjector hologramProjector;
            public float hologramDistance = 0f;

            public void Awake()
            {
                modelLocator = GetComponent<ModelLocator>();
                if (modelLocator)
                {
                    foreach (var renderer in modelLocator.modelTransform.GetComponentsInChildren<MeshRenderer>())
                    {
                        materialsPerRenderer[renderer] = renderer.sharedMaterials.ToArray();
                        renderer.sharedMaterials = new Material[] { cloakedMaterial };
                    }
                    foreach (var renderer in modelLocator.modelTransform.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        materialsPerRenderer[renderer] = renderer.sharedMaterials.ToArray();
                        renderer.sharedMaterials = new Material[] { cloakedMaterial };
                    }

                    hologramProjector = GetComponent<RoR2.Hologram.HologramProjector>();
                    if (hologramProjector)
                    {
                        hologramDistance = hologramProjector.displayDistance;
                        hologramProjector.displayDistance = 0f;
                    }
                }

                InstanceTracker.Add(this);
            }

            public void OnDestroy()
            {
                InstanceTracker.Remove(this);

                if (modelLocator)
                {
                    foreach (var renderer in materialsPerRenderer.Keys)
                    {
                        renderer.sharedMaterials = materialsPerRenderer[renderer];
                    }

                    if (hologramProjector)
                    {
                        hologramProjector.displayDistance = hologramDistance;
                    }
                }
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            On.RoR2.PurchaseInteraction.Awake -= PurchaseInteraction_Awake;
            foreach (var cloakedChest in InstanceTracker.GetInstancesList<ChallengeModeCloakedChest>().ToList())
            {
                Object.Destroy(cloakedChest);
            }
        }

        public override bool IsAvailable()
        {
            return ChallengeModeUtils.CurrentStageHasCommonInteractables();
        }
    }
}