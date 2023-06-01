using BepInEx;
using MysticsRisky2Utils;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace ChallengeMode
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(R2API.R2API.PluginGUID + ".elites")]
    [BepInDependency(R2API.R2API.PluginGUID + ".damagetype")]
    [BepInDependency(R2API.R2API.PluginGUID + ".dot")]
    [BepInDependency(R2API.R2API.PluginGUID + ".language")]
    [BepInDependency(R2API.R2API.PluginGUID + ".networking")]
    [BepInDependency(R2API.R2API.PluginGUID + ".orb")]
    [BepInDependency(R2API.R2API.PluginGUID + ".prefab")]
    [BepInDependency(R2API.R2API.PluginGUID + ".recalculatestats")]
    [BepInDependency(MysticsRisky2UtilsPlugin.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class ChallengeModePlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.themysticsword.challengemode";
        public const string PluginName = "Virga";
        public const string PluginVersion = "1.0.0";

        public static System.Reflection.Assembly executingAssembly;
        internal static System.Type declaringType;
        internal static PluginInfo pluginInfo;
        internal static BepInEx.Logging.ManualLogSource logger;
        internal static BepInEx.Configuration.ConfigFile config;

        private static AssetBundle _assetBundle;
        public static AssetBundle AssetBundle
        {
            get
            {
                if (_assetBundle == null)
                    _assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pluginInfo.Location), "challengemodeassetbundle"));
                return _assetBundle;
            }
        }

        public void Awake()
        {
            pluginInfo = Info;
            logger = Logger;
            config = Config;
            executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            declaringType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

            if (MysticsRisky2Utils.SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.enabled)
            {
                Sprite iconSprite = null;
                var iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "icon.png");
                if (System.IO.File.Exists(iconPath))
                {
                    var iconTexture = new Texture2D(2, 2);
                    iconTexture.LoadImage(System.IO.File.ReadAllBytes(iconPath));
                    iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0, 0), 100);
                }
                MysticsRisky2Utils.SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.RegisterModInfo(PluginGUID, PluginName, "Challenge game mode with random modifiers", iconSprite);
            }

            ChallengeModeConfig.Init();
            ChallengeRunModifierCatalog.Init();
            TMProEffects.Init();

            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseItem>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseEquipment>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseBuff>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseInteractable>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<ChallengeRunLoadable>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<BrotherEXAssets>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<VoidRaidCrabEXAssets>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<PetGame>(executingAssembly);

            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new ChallengeModeContent());
            };
        }
    }

    public class ChallengeModeContent : IContentPackProvider
    {
        public string identifier
        {
            get
            {
                return ChallengeModePlugin.PluginName;
            }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper contentLoadHelper = new MysticsRisky2Utils.ContentManagement.ContentLoadHelper();

            // Add content loading dispatchers to the content load helper
            System.Action[] loadDispatchers = new System.Action[]
            {
                () => contentLoadHelper.DispatchLoad<ItemDef>(ChallengeModePlugin.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseItem), x => contentPack.itemDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<EquipmentDef>(ChallengeModePlugin.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseEquipment), x => contentPack.equipmentDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<BuffDef>(ChallengeModePlugin.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseBuff), x => contentPack.buffDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseInteractable), null),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody), x => contentPack.bodyPrefabs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster), x => contentPack.masterPrefabs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(ChallengeRunLoadable), x => contentPack.gameModePrefabs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(EXBossAssets), null),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(BrotherEXAssets), null),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(VoidRaidCrabEXAssets), null),
                () => contentLoadHelper.DispatchLoad<GameObject>(ChallengeModePlugin.executingAssembly, typeof(PetGame), null),
                () => PostProcessing.ChallengeModePostProcessing.Init(),
                () => Modifiers.Unique.Thunder.InitAssets()
            };
            int num = 0;
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0f, 0.05f));
                yield return null;
                num = i + 1;
            }

            // Start loading content. Longest part of the loading process, so we will dedicate most of the progress bar to it
            while (contentLoadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(Util.Remap(contentLoadHelper.progress.value, 0f, 1f, 0.05f, 0.9f));
                yield return contentLoadHelper.coroutine.Current;
            }

            // Populate static content pack fields and add various prefabs and scriptable objects generated during the content loading part to the content pack
            loadDispatchers = new System.Action[]
            {
                () => ContentLoadHelper.PopulateTypeFields<ItemDef>(typeof(Items), contentPack.itemDefs),
                () => ContentLoadHelper.PopulateTypeFields<BuffDef>(typeof(Buffs), contentPack.buffDefs),
                () => contentPack.bodyPrefabs.Add(Resources.bodyPrefabs.ToArray()),
                () => contentPack.masterPrefabs.Add(Resources.masterPrefabs.ToArray()),
                () => contentPack.projectilePrefabs.Add(Resources.projectilePrefabs.ToArray()),
                () => contentPack.gameModePrefabs.Add(Resources.gameModePrefabs.ToArray()),
                () => contentPack.networkedObjectPrefabs.Add(Resources.networkedObjectPrefabs.ToArray()),
                () => contentPack.effectDefs.Add(Resources.effectPrefabs.ConvertAll(x => new EffectDef(x)).ToArray()),
                () => contentPack.networkSoundEventDefs.Add(Resources.networkSoundEventDefs.ToArray()),
                () => contentPack.unlockableDefs.Add(Resources.unlockableDefs.ToArray()),
                () => contentPack.entityStateTypes.Add(Resources.entityStateTypes.ToArray()),
                () => contentPack.skillDefs.Add(Resources.skillDefs.ToArray()),
                () => contentPack.skillFamilies.Add(Resources.skillFamilies.ToArray()),
                () => contentPack.sceneDefs.Add(Resources.sceneDefs.ToArray()),
                () => contentPack.gameEndingDefs.Add(Resources.gameEndingDefs.ToArray())
            };
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0.9f, 0.95f));
                yield return null;
                num = i + 1;
            }

            // Call "AfterContentPackLoaded" methods
            loadDispatchers = new System.Action[]
            {
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseItem>(ChallengeModePlugin.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseEquipment>(ChallengeModePlugin.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseBuff>(ChallengeModePlugin.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseInteractable>(ChallengeModePlugin.executingAssembly)
            };
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0.95f, 0.99f));
                yield return null;
                num = i + 1;
            }

            loadDispatchers = null;
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private ContentPack contentPack = new ContentPack();

        public static class Resources
        {
            public static List<GameObject> bodyPrefabs = new List<GameObject>();
            public static List<GameObject> masterPrefabs = new List<GameObject>();
            public static List<GameObject> projectilePrefabs = new List<GameObject>();
            public static List<GameObject> effectPrefabs = new List<GameObject>();
            public static List<GameObject> gameModePrefabs = new List<GameObject>();
            public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();
            public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
            public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();
            public static List<System.Type> entityStateTypes = new List<System.Type>();
            public static List<RoR2.Skills.SkillDef> skillDefs = new List<RoR2.Skills.SkillDef>();
            public static List<RoR2.Skills.SkillFamily> skillFamilies = new List<RoR2.Skills.SkillFamily>();
            public static List<SceneDef> sceneDefs = new List<SceneDef>();
            public static List<GameEndingDef> gameEndingDefs = new List<GameEndingDef>();
        }

        public static class Items
        {
            public static ItemDef ChallengeMode_PermanentImmuneToVoidDeath;
        }

        public static class Buffs
        {
            public static BuffDef ChallengeMode_BackpackWeight;
            public static BuffDef ChallengeMode_BlockNextDamage;
            public static BuffDef ChallengeMode_CommsJammed;
            public static BuffDef ChallengeMode_CommsJammedVisuals;
            public static BuffDef ChallengeMode_CurseCooldown;
            public static BuffDef ChallengeMode_DefenseDamage;
            public static BuffDef ChallengeMode_Disarmed;
            public static BuffDef ChallengeMode_EXBoss;
            public static BuffDef ChallengeMode_Frostbite;
            public static BuffDef ChallengeMode_KillRemorse;
            public static BuffDef ChallengeMode_LowHPStress;
            public static BuffDef ChallengeMode_PlayerKnockupStun;
            public static BuffDef ChallengeMode_ProcLimitIndicator;
            public static BuffDef ChallengeMode_Stamina;
            public static BuffDef ChallengeMode_SuperArmor;
            public static BuffDef ChallengeMode_SuperArmorGained;
            public static BuffDef ChallengeMode_VoidRaidCrabScare;
        }
    }
}
