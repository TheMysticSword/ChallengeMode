using MysticsRisky2Utils;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode
{
    public class BrotherEXAssets : BaseLoadableAsset
    {
        public static GameObject arenaEffectsPrefab;

        public static DamageAPI.ModdedDamageType destroyItemDamageType;
        public static SkillDef playerStunLockedSkill;

        public static GameObject skyLeapHomingController;

        public static GameObject leapWarningCircle;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            destroyItemDamageType = DamageAPI.ReserveDamageType();
            skyLeapHomingController = Utils.CreateBlankPrefab("ChallengeMode_SkyLeapHomingController", true);
            skyLeapHomingController.AddComponent<NetworkTransform>();
            skyLeapHomingController.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            skyLeapHomingController.AddComponent<ChallengeModeSkyLeapHomingController>();
        }

        public override void Load()
        {
            arenaEffectsPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/BrotherEX/BrotherEXArenaEffects.prefab");

            playerStunLockedSkill = ScriptableObject.CreateInstance<SkillDef>();
            playerStunLockedSkill.skillName = "ChallengeMode_StunLocked";
            playerStunLockedSkill.skillNameToken = "CHALLENGEMODE_SKILL_PLAYERSTUNLOCKED_NAME";
            playerStunLockedSkill.skillDescriptionToken = "CHALLENGEMODE_SKILL_PLAYERSTUNLOCKED_DESCRIPTION";
            playerStunLockedSkill.icon = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Engi/EngiCancelTargetingDummy.asset").WaitForCompletion().icon;
            playerStunLockedSkill.activationStateMachineName = "Weapon";
            playerStunLockedSkill.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle));
            playerStunLockedSkill.interruptPriority = EntityStates.InterruptPriority.Any;
            playerStunLockedSkill.baseRechargeInterval = 0f;
            playerStunLockedSkill.baseMaxStock = 0;
            playerStunLockedSkill.rechargeStock = 0;
            playerStunLockedSkill.requiredStock = 0;
            playerStunLockedSkill.stockToConsume = 0;
            playerStunLockedSkill.resetCooldownTimerOnUse = false;
            playerStunLockedSkill.fullRestockOnAssign = false;
            playerStunLockedSkill.dontAllowPastMaxStocks = false;
            playerStunLockedSkill.beginSkillCooldownOnSkillEnd = false;
            playerStunLockedSkill.cancelSprintingOnActivation = false;
            playerStunLockedSkill.forceSprintDuringState = false;
            playerStunLockedSkill.canceledFromSprinting = false;
            playerStunLockedSkill.isCombatSkill = false;
            playerStunLockedSkill.mustKeyPress = true;
            ChallengeModeContent.Resources.skillDefs.Add(playerStunLockedSkill);

            GenericGameEvents.OnTakeDamage += GenericGameEvents_OnTakeDamage;

            leapWarningCircle = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/Modifiers/BrotherEX/SkyLeapWarning.prefab");
            
            OnLoad();
            asset = arenaEffectsPrefab;
        }

        private static void GenericGameEvents_OnTakeDamage(DamageReport damageReport)
        {
            if (NetworkServer.active && DamageAPI.HasModdedDamageType(damageReport.damageInfo, destroyItemDamageType) && damageReport.victimMaster && damageReport.victimMaster.inventory)
            {
                var items = damageReport.victimMaster.inventory.itemAcquisitionOrder.Select(x => ItemCatalog.GetItemDef(x)).Where(x => x != null && x.canRemove && !x.hidden).ToList();
                if (items.Count > 0)
                {
                    var randomItem = RoR2Application.rng.NextElementUniform(items);
                    damageReport.victimMaster.inventory.RemoveItem(randomItem);
                    damageReport.victimMaster.inventory.GiveItem(RoR2Content.Items.ScrapWhite);
                    CharacterMasterNotificationQueue.SendTransformNotification(
                        damageReport.victimMaster,
                        randomItem.itemIndex,
                        RoR2Content.Items.ScrapWhite.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default
                    );
                }
            }
        }

        public class ChallengeModeSkyLeapHomingController : MonoBehaviour
        {
            public CharacterBody body;

            public GameObject warningCircle;
            public Transform warningCircleTransform;

            public float leapAcceleration = 40f;
            public float leapMaxSpeed = 30f;
            public Vector3 leapSpeed = Vector3.zero;
            public Vector3 homingPosition = Vector3.zero;
            private Vector3 spareVector = Vector3.zero;

            public void Start()
            {
                warningCircle = Object.Instantiate(leapWarningCircle, new Vector3(transform.position.x, 0f, transform.position.z), Quaternion.identity);
                warningCircleTransform = warningCircle.transform;
            }

            public void FixedUpdate()
            {
                if (body && body.master.aiComponents.Length > 0 && body.master.aiComponents[0].currentEnemy != null)
                {
                    homingPosition = (Vector3)body.master.aiComponents[0].currentEnemy.lastKnownBullseyePosition;
                    homingPosition.y = 0f;
                }

                /*
                leapSpeed.x += leapAcceleration * Time.fixedDeltaTime * (homingPosition.x > transform.position.x ? 1f : -1f);
                if (Mathf.Abs(leapSpeed.x) > leapMaxSpeed) leapSpeed.x = Mathf.Sign(leapSpeed.x) * leapMaxSpeed;
                leapSpeed.z += leapAcceleration * Time.fixedDeltaTime * (homingPosition.z > transform.position.z ? 1f : -1f);
                if (Mathf.Abs(leapSpeed.z) > leapMaxSpeed) leapSpeed.z = Mathf.Sign(leapSpeed.z) * leapMaxSpeed;
                */
                var myPosition2D = transform.position;
                myPosition2D.y = 0f;
                leapSpeed += (homingPosition - myPosition2D).normalized * leapAcceleration * Time.fixedDeltaTime;
                if (Mathf.Abs(leapSpeed.x) > leapMaxSpeed) leapSpeed.x = Mathf.Sign(leapSpeed.x) * leapMaxSpeed;
                if (Mathf.Abs(leapSpeed.z) > leapMaxSpeed) leapSpeed.z = Mathf.Sign(leapSpeed.z) * leapMaxSpeed;

                transform.position += leapSpeed * Time.fixedDeltaTime;
                if (body && body.hasEffectiveAuthority)
                {
                    body.characterMotor.Motor.SetPosition(new Vector3(
                        transform.position.x,
                        body.transform.position.y,
                        transform.position.z
                    ));
                }

                var groundPoint = 491f;
                if (Physics.Raycast(new Ray(new Vector3(transform.position.x, 560f, transform.position.z), Vector3.down), out var hitInfo, 500f, LayerIndex.world.mask, QueryTriggerInteraction.UseGlobal))
                {
                    groundPoint = hitInfo.point.y;
                }
                spareVector.x = transform.position.x;
                spareVector.y = groundPoint;
                spareVector.z = transform.position.z;
                transform.position = spareVector;
                warningCircleTransform.position = spareVector;
            }

            public void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            public void OnDisable()
            {
                InstanceTracker.Remove(this);
            }

            public void OnDestroy()
            {
                if (warningCircle) Destroy(warningCircle);
            }
        }
    }
}
