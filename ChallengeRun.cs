using MysticsRisky2Utils;
using MysticsRisky2Utils.ContentManagement;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using TMPro;

namespace ChallengeMode
{
    public class ChallengeRunLoadable : BaseLoadableAsset
    {
        public static GameObject challengeRunPrefab;
        public static GameObject modifierListPrefab;

        public static GameObject extraGameModeMenu;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            challengeRunPrefab = Utils.CreateBlankPrefab("xChallengeRun", true); // the x at the start is needed to put the gamemode at the end of the catalog
            NetworkingAPI.RegisterMessageType<ChallengeRun.SyncSetNewModifiers>();
            NetworkingAPI.RegisterMessageType<ChallengeRun.SyncDisableAllModifiers>();

            On.RoR2.GameModeCatalog.SetGameModes += GameModeCatalog_SetGameModes; // the catalog is also not sorted by the game properly
        }

        private void GameModeCatalog_SetGameModes(On.RoR2.GameModeCatalog.orig_SetGameModes orig, Run[] newGameModePrefabComponents)
        {
            System.Array.Sort<Run>(newGameModePrefabComponents, (Run a, Run b) => string.CompareOrdinal(a.name, b.name));
            orig(newGameModePrefabComponents);
        }

        public override void Load()
        {
            var classicRun = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion().GetComponent<Run>();

            var run = challengeRunPrefab.AddComponent<ChallengeRun>();
            run.nameToken = "GAMEMODE_CHALLENGEMODE_NAME";
            run.userPickable = true;
            run.startingSceneGroup = classicRun.startingSceneGroup;
            run.gameOverPrefab = classicRun.gameOverPrefab;
            run.lobbyBackgroundPrefab = classicRun.lobbyBackgroundPrefab;
            run.uiPrefab = classicRun.uiPrefab;

            challengeRunPrefab.AddComponent<TeamManager>();
            challengeRunPrefab.AddComponent<RunCameraManager>();

            SetUpAlternateGameModeButton();
            SetUpModifierList();

            OnLoad();

            asset = challengeRunPrefab;
        }

        public void SetUpAlternateGameModeButton()
        {
            On.RoR2.UI.LanguageTextMeshController.Start += LanguageTextMeshController_Start;
        }

        private void LanguageTextMeshController_Start(On.RoR2.UI.LanguageTextMeshController.orig_Start orig, LanguageTextMeshController self)
        {
            orig(self);
            if (self.token == "TITLE_ECLIPSE" && self.GetComponent<HGButton>())
            {
                self.transform.parent.gameObject.AddComponent<ChallengeRunButtonAdder>();
            }
        }

        public void SetUpModifierList()
        {
            var classicRunInfoHudPanel = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ClassicRun/ClassicRunInfoHudPanel.prefab").WaitForCompletion();

            modifierListPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/ModifierListPanel.prefab");

            modifierListPrefab.GetComponent<UnityEngine.UI.Image>().sprite = classicRunInfoHudPanel.transform.Find("RightInfoBar/ObjectivePanel").GetComponent<UnityEngine.UI.Image>().sprite;
            modifierListPrefab.transform.Find("StripContainer/ModifierStrip").GetComponent<UnityEngine.UI.Image>().sprite = classicRunInfoHudPanel.transform.Find("RightInfoBar/ObjectivePanel/StripContainer/ObjectiveStrip").GetComponent<UnityEngine.UI.Image>().sprite;
            modifierListPrefab.transform.Find("StripContainer/ModifierStrip").GetComponent<UnityEngine.UI.Image>().material = classicRunInfoHudPanel.transform.Find("RightInfoBar/ObjectivePanel/StripContainer/ObjectiveStrip").GetComponent<UnityEngine.UI.Image>().material;

            void FixLabel(GameObject labelObject, RoR2.UI.SkinControllers.LabelSkinController.LabelType labelType)
            {
                var tmp = labelObject.GetComponent<TextMeshProUGUI>();
                var alignment = tmp.alignment;
                Object.DestroyImmediate(tmp);
                var hgTmp = labelObject.AddComponent<HGTextMeshProUGUI>();
                hgTmp.alignment = alignment;
                hgTmp.enableWordWrapping = true;

                var skinController = labelObject.AddComponent<RoR2.UI.SkinControllers.LabelSkinController>();
                skinController.labelType = labelType;
                skinController.useRecommendedAlignment = false;
                skinController.skinData = Addressables.LoadAssetAsync<UISkinData>("RoR2/Base/UI/skinObjectivePanel.asset").WaitForCompletion();

                skinController.label = labelObject.GetComponent<HGTextMeshProUGUI>();
                skinController.DoSkinUI();
            }
            FixLabel(modifierListPrefab.transform.Find("Label").gameObject, RoR2.UI.SkinControllers.LabelSkinController.LabelType.Header);
            modifierListPrefab.transform.Find("Label").gameObject.AddComponent<LanguageTextMeshController>()._token = "CHALLENGEMODE_MODIFIER_LIST_TITLE";
            FixLabel(modifierListPrefab.transform.Find("StripContainer/ModifierStrip/Name").gameObject, RoR2.UI.SkinControllers.LabelSkinController.LabelType.Default);
            modifierListPrefab.transform.Find("StripContainer/ModifierStrip/Name").gameObject.AddComponent<LanguageTextMeshController>();
            FixLabel(modifierListPrefab.transform.Find("StripContainer/ModifierStrip/Description").gameObject, RoR2.UI.SkinControllers.LabelSkinController.LabelType.Detail);
            modifierListPrefab.transform.Find("StripContainer/ModifierStrip/Description").gameObject.AddComponent<LanguageTextMeshController>();
            var controller = modifierListPrefab.AddComponent<ChallengeRunModifierListPanelController>();
            controller.stripPrefab = modifierListPrefab.transform.Find("StripContainer/ModifierStrip").gameObject;
            controller.stripContainer = modifierListPrefab.transform.Find("StripContainer");
        }
    }

    public class ChallengeRunButtonAdder : MonoBehaviour
    {
        public void Start()
        {
            var newButton = Object.Instantiate(transform.Find("GenericMenuButton (Eclipse)").gameObject, transform);
            newButton.AddComponent<ChallengeRunButton>();
            newButton.GetComponent<LanguageTextMeshController>().token = "TITLE_CHALLENGEMODE";
            newButton.GetComponent<HGButton>().hoverToken = "TITLE_CHALLENGEMODE_DESC";
        }
    }

    public class ChallengeRunButton : MonoBehaviour
    {
        public HGButton hgButton;

        public void Start()
        {
            hgButton = GetComponent<HGButton>();
            hgButton.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            hgButton.onClick.AddListener(() =>
            {
                Util.PlaySound("Play_UI_menuClick", RoR2Application.instance.gameObject);
                Console.instance.SubmitCmd(null, "transition_command \"gamemode xChallengeRun; host 0; \"", false);
            });
        }
    }

    public class ChallengeRun : Run
    {
        public Xoroshiro128Plus modifierRng;
        
        private int networkingCycle = 0;

        public List<ChallengeRunModifier> currentModifiers = new List<ChallengeRunModifier>();
        public List<ChallengeRunModifier> modifierPool = new List<ChallengeRunModifier>();
        public List<ChallengeRunModifier> additionalModifierPool = new List<ChallengeRunModifier>();

        public float markHUDDirtyTimer = 0f;
        public float markHUDDirtyInterval = 1f;
        public float markHUDDirtyLimit = 15f;

        public override void Start()
        {
            Stage.onServerStageBegin += Stage_onServerStageBegin;
            Stage.onServerStageComplete += Stage_onServerStageComplete;
            HUD.onHudTargetChangedGlobal += HUD_onHudTargetChangedGlobal;
            base.Start();
            if (NetworkServer.active)
            {
                GenerateStageRNGChallengeRun();
            }
        }

        public override void OnDestroy()
        {
            Stage.onServerStageBegin -= Stage_onServerStageBegin;
            Stage.onServerStageComplete -= Stage_onServerStageComplete;
            HUD.onHudTargetChangedGlobal -= HUD_onHudTargetChangedGlobal;
            DisableAllModifiers();
            base.OnDestroy();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (markHUDDirtyLimit > 0f)
            {
                markHUDDirtyLimit -= Time.fixedDeltaTime;
                markHUDDirtyTimer += Time.fixedDeltaTime;
                if (markHUDDirtyTimer >= markHUDDirtyInterval)
                {
                    markHUDDirtyTimer -= markHUDDirtyInterval;
                    ChallengeRunModifierListPanelController.MarkDirty();
                }
            }
        }

        public override void AdvanceStage(SceneDef nextScene)
        {
            base.AdvanceStage(nextScene);
            GenerateStageRNGChallengeRun();
        }

        public void GenerateStageRNGChallengeRun()
        {
            modifierRng = new Xoroshiro128Plus(stageRng.nextUlong);
        }

        public override void OnClientGameOver(RunReport runReport)
        {
            base.OnClientGameOver(runReport);
            if (runReport.gameEnding.isWin)
            {
                var petFoodReward = RoR2Application.rng.RangeInt(5, 15);
                if (loopClearCount > 0)
                    petFoodReward *= loopClearCount;
                var difficultyDef = DifficultyCatalog.GetDifficultyDef(selectedDifficulty);
                if (difficultyDef != null)
                    petFoodReward = Mathf.CeilToInt(petFoodReward * difficultyDef.scalingValue);
                ChallengeModeConfig.petFood.Value += petFoodReward;
            }
        }

        private void Stage_onServerStageBegin(Stage stage)
        {
            RefreshModifierPools();
            AddRandomNewModifiersForThisStageServer();
            /*
            if (ChallengeModeUtils.CurrentStageNameMatches("moon2"))
            {
                AddNewModifierServer(ChallengeRunModifierCatalog.nameToModifier["BrotherEX"]);
            }
            */
        }

        private void Stage_onServerStageComplete(Stage stage)
        {
            DisableAllModifiers();
        }

        private void HUD_onHudTargetChangedGlobal(HUD hud)
        {
            ChallengeRunModifierListPanelController.MarkDirty();
        }

        public void RefreshModifierPools()
        {
            modifierPool.Clear();
            additionalModifierPool.Clear();
            for (var i = 0; i < ChallengeRunModifierCatalog.allModifiers.Count; i++)
            {
                var modifier = ChallengeRunModifierCatalog.allModifiers[i];
                if (modifier.IsAvailable() && modifier.isEnabledInConfig)
                {
                    if (modifier.isAdditional)
                    {
                        additionalModifierPool.Add(modifier);
                    }
                    else
                    {
                        modifierPool.Add(modifier);
                    }
                }
            }
        }

        public void AddRandomNewModifiersForThisStageServer()
        {
            if (NetworkServer.active)
            {
                var newModifiers = new List<ChallengeRunModifier>();
                for (var i = 0; i < ChallengeModeConfig.modifiersPerStage; i++)
                {
                    if (modifierPool.Count <= 0) break;
                    var modifier = modifierRng.NextElementUniform(modifierPool);
                    newModifiers.Add(modifier);
                    modifierPool.Remove(modifier);
                }
                foreach (var modifier in additionalModifierPool)
                {
                    newModifiers.Add(modifier);
                }

                AddNewModifiersServer(newModifiers);
            }
        }

        public void AddNewModifierServer(ChallengeRunModifier newModifier)
        {
            AddNewModifiersServer(new List<ChallengeRunModifier> { newModifier });
        }

        public void AddNewModifiersServer(List<ChallengeRunModifier> newModifiers)
        {
            if (NetworkServer.active)
            {
                foreach (var modifier in newModifiers)
                {
                    try
                    {
                        modifier.OnEnable();
                    }
                    catch (System.Exception e)
                    {
                        ChallengeModePlugin.logger.LogError("Error while enabling modifier " + modifier.GetType().Name);
                        ChallengeModePlugin.logger.LogError(e);
                    }
                    currentModifiers.Add(modifier);
                }

                ChallengeRunModifierListPanelController.MarkDirty();

                new SyncSetNewModifiers(gameObject.GetComponent<NetworkIdentity>().netId, networkingCycle, currentModifiers.Select(x => x.modifierIndex).ToList()).Send(NetworkDestination.Clients);
                networkingCycle++;
            }
        }

        public void DisableAllModifiers()
        {
            foreach (var modifier in currentModifiers)
            {
                try
                {
                    modifier.OnDisable();
                }
                catch (System.Exception e)
                {
                    ChallengeModePlugin.logger.LogError("Error while disabling modifier " + modifier.GetType().Name);
                    ChallengeModePlugin.logger.LogError(e);
                }
            }
            currentModifiers.Clear();

            ChallengeRunModifierListPanelController.MarkDirty();

            if (NetworkServer.active)
            {
                new SyncDisableAllModifiers(gameObject.GetComponent<NetworkIdentity>().netId, networkingCycle).Send(NetworkDestination.Clients);
                networkingCycle++;
            }
        }

        public class SyncSetNewModifiers : INetMessage
        {
            NetworkInstanceId objID;
            int networkingCycle;
            List<int> modifierIndices;

            public SyncSetNewModifiers()
            {
            }

            public SyncSetNewModifiers(NetworkInstanceId objID, int networkingCycle, List<int> modifierIndices)
            {
                this.objID = objID;
                this.networkingCycle = networkingCycle;
                this.modifierIndices = modifierIndices;
            }

            public void Deserialize(NetworkReader reader)
            {
                objID = reader.ReadNetworkId();
                networkingCycle = reader.ReadInt32();
                var count = reader.ReadInt32();
                modifierIndices = new List<int>();
                for (var i = 0; i < count; i++)
                {
                    modifierIndices.Add(reader.ReadInt32());
                }
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                GameObject obj = Util.FindNetworkObject(objID);
                if (obj)
                {
                    var run = obj.GetComponent<ChallengeRun>();
                    if (run)
                    {
                        if (run.networkingCycle < networkingCycle)
                        {
                            run.networkingCycle = networkingCycle;
                            run.DisableAllModifiers();
                            foreach (var modifierIndex in modifierIndices)
                            {
                                if (ChallengeRunModifierCatalog.indexToModifier.TryGetValue(modifierIndex, out var modifier))
                                {
                                    modifier.OnEnable();
                                    run.currentModifiers.Add(modifier);
                                }
                            }
                            ChallengeRunModifierListPanelController.MarkDirty();
                        }
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(objID);
                writer.Write(networkingCycle);
                writer.Write(modifierIndices.Count);
                foreach (var modifierIndex in modifierIndices)
                {
                    writer.Write(modifierIndex);
                }
            }
        }

        public class SyncDisableAllModifiers : INetMessage
        {
            NetworkInstanceId objID;
            int networkingCycle;

            public SyncDisableAllModifiers()
            {
            }

            public SyncDisableAllModifiers(NetworkInstanceId objID, int networkingCycle)
            {
                this.objID = objID;
                this.networkingCycle = networkingCycle;
            }

            public void Deserialize(NetworkReader reader)
            {
                objID = reader.ReadNetworkId();
                networkingCycle = reader.ReadInt32();
            }

            public void OnReceived()
            {
                if (NetworkServer.active) return;
                GameObject obj = Util.FindNetworkObject(objID);
                if (obj)
                {
                    var run = obj.GetComponent<ChallengeRun>();
                    if (run)
                    {
                        if (run.networkingCycle < networkingCycle)
                        {
                            run.networkingCycle = networkingCycle;
                            run.DisableAllModifiers();
                        }
                    }
                }
            }

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(objID);
                writer.Write(networkingCycle);
            }
        }

        [ConCommand(commandName = "challengemode_addmodifier", flags = ConVarFlags.None, helpText = "Activates a modifier by internal name.")]
        public static void CCAddModifier(ConCommandArgs args)
        {
            if (!NetworkServer.active)
            {
                throw new System.Exception("This is a server-side command.");
            }

            var challengeRun = Run.instance as ChallengeRun;
            if (challengeRun == null)
            {
                throw new System.Exception("Not in a ChallengeRun!");
            }

            var modifierName = args.GetArgString(0);
            ChallengeRunModifier modifier = null;
            if (!ChallengeRunModifierCatalog.nameToModifier.TryGetValue(modifierName, out modifier))
            {
                throw new System.Exception(string.Format("Modifier not found. modifierName={0}", modifierName));
            }
            
            if (!challengeRun.currentModifiers.Contains(modifier))
            {
                challengeRun.AddNewModifierServer(modifier);
            }
        }

        [ConCommand(commandName = "challengemode_disableallmodifiers", flags = ConVarFlags.None, helpText = "Disables all modifiers.")]
        public static void CCDisableAllModifiers(ConCommandArgs args)
        {
            if (!NetworkServer.active)
            {
                throw new System.Exception("This is a server-side command.");
            }

            var challengeRun = Run.instance as ChallengeRun;
            if (challengeRun == null)
            {
                throw new System.Exception("Not in a ChallengeRun!");
            }

            challengeRun.DisableAllModifiers();
        }

        [ConCommand(commandName = "challengemode_rerollmodifiers", flags = ConVarFlags.None, helpText = "Rerolls current modifiers.")]
        public static void CCRerollModifiers(ConCommandArgs args)
        {
            if (!NetworkServer.active)
            {
                throw new System.Exception("This is a server-side command.");
            }

            var challengeRun = Run.instance as ChallengeRun;
            if (challengeRun == null)
            {
                throw new System.Exception("Not in a ChallengeRun!");
            }

            challengeRun.DisableAllModifiers();
            challengeRun.RefreshModifierPools();
            challengeRun.AddRandomNewModifiersForThisStageServer();
        }
    }

    public class ChallengeRunModifierListPanelController : MonoBehaviour
    {
        public HUD hud;
        public GameObject stripPrefab;
        public Transform stripContainer;

        public class ModifierStrip
        {
            public ChallengeRunModifier modifier;
            public GameObject gameObject;
        }

        public List<ModifierStrip> strips = new List<ModifierStrip>();
        public List<StripEnterAnimation> stripEnterAnimations = new List<StripEnterAnimation>();

        public void OnEnable()
        {
            InstanceTracker.Add(this);
        }

        public void OnDisable()
        {
            InstanceTracker.Remove(this);
        }

        public void Update()
        {
            if (stripEnterAnimations.Count > 0)
            {
                var duration = 2f;
                var addedTime = Time.deltaTime / duration;
                for (var i = stripEnterAnimations.Count - 1; i >= 0; i--)
                {
                    var newAnimationTime = Mathf.Min(stripEnterAnimations[i].t + addedTime, stripEnterAnimations[i].maxT);
                    stripEnterAnimations[i].SetT(newAnimationTime);
                    if (newAnimationTime >= stripEnterAnimations[i].maxT)
                    {
                        stripEnterAnimations.RemoveAt(i);
                    }
                }
            }
        }

        public void SetModifiers(List<ChallengeRunModifier> newModifiers)
        {
            foreach (var strip in strips)
            {
                if (!newModifiers.Contains(strip.modifier))
                    Destroy(strip.gameObject);
            }
            strips.RemoveAll(x => !newModifiers.Contains(x.modifier));
            stripEnterAnimations.RemoveAll(x => x.strip.gameObject == null);

            var i = 0;
            foreach (var modifier in newModifiers)
                if (!strips.Any(x => x.modifier == modifier))
                {
                    var newStrip = new ModifierStrip();
                    newStrip.modifier = modifier;
                    newStrip.gameObject = Instantiate(stripPrefab, stripContainer);
                    newStrip.gameObject.SetActive(true);
                    newStrip.gameObject.transform.Find("Name").GetComponent<LanguageTextMeshController>().token = modifier.nameToken;
                    newStrip.gameObject.transform.Find("Description").GetComponent<LanguageTextMeshController>().token = modifier.descriptionToken;
                    strips.Add(newStrip);

                    var enterAnimation = new StripEnterAnimation(newStrip);
                    enterAnimation.maxT += 0.2f * i;
                    stripEnterAnimations.Add(enterAnimation);

                    i++;
                }
        }

        public class StripEnterAnimation
        {
            public float t;
            public float maxT = 1f;
            public float finalHeight;
            public UnityEngine.UI.LayoutElement layoutElement;
            public CanvasGroup canvasGroup;
            public ModifierStrip strip;

            public StripEnterAnimation(ModifierStrip strip)
            {
                this.strip = strip;
                layoutElement = strip.gameObject.GetComponent<UnityEngine.UI.LayoutElement>();
                canvasGroup = strip.gameObject.GetComponent<CanvasGroup>();
                finalHeight = layoutElement.minHeight;
            }

            public void SetT(float newT)
            {
                if (strip.gameObject)
                {
                    t = newT;
                    var alpha = Mathf.Clamp01(Util.Remap(t / maxT, 0.75f, 1f, 0f, 1f));
                    canvasGroup.alpha = alpha;
                    var heightCoeff = Mathf.Clamp01(Util.Remap(t / maxT, 0.5f, 0.75f, 0f, 1f));
                    heightCoeff *= heightCoeff;
                    layoutElement.minHeight = heightCoeff * finalHeight;
                    layoutElement.preferredHeight = layoutElement.minHeight;
                    layoutElement.flexibleHeight = 0f;
                }
            }
        }

        public static bool isDirty = false;

        public static void MarkDirty()
        {
            if (isDirty) return;
            isDirty = true;
            RoR2Application.onNextUpdate += RefreshAll;
        }

        private static void RefreshAll()
        {
            for (int i = 0; i < HUD.readOnlyInstanceList.Count; i++)
            {
                RefreshHUD(HUD.readOnlyInstanceList[i]);
            }
            isDirty = false;
        }

        private static void RefreshHUD(HUD hud)
        {
            if (!hud.targetMaster) return;

            var challengeRun = Run.instance as ChallengeRun;
            if (challengeRun != null)
            {
                SetDisplayDataForViewer(hud, challengeRun.currentModifiers);
            }
        }

        private static void SetDisplayDataForViewer(HUD hud, List<ChallengeRunModifier> modifiers)
        {
            bool shouldDisplay = modifiers.Count > 0;
            var controller = SetDisplayingOnHud(hud, shouldDisplay);
            if (controller)
            {
                controller.SetModifiers(modifiers);
            }
        }

        private static ChallengeRunModifierListPanelController SetDisplayingOnHud(HUD hud, bool shouldDisplay)
        {
            var instancesList = InstanceTracker.GetInstancesList<ChallengeRunModifierListPanelController>();
            ChallengeRunModifierListPanelController controller = null;
            for (int i = 0; i < instancesList.Count; i++)
            {
                var controller2 = instancesList[i];
                if (controller2.hud == hud)
                {
                    controller = controller2;
                    break;
                }
            }
            if (controller != shouldDisplay)
            {
                if (!controller)
                {
                    Transform transform = null;
                    if (hud.gameModeUiInstance)
                    {
                        ChildLocator component = hud.gameModeUiInstance.GetComponent<ChildLocator>();
                        if (component)
                        {
                            Transform transform2 = component.FindChild("RightInfoBar");
                            if (transform2)
                            {
                                transform = transform2.GetComponent<RectTransform>();
                            }
                        }
                    }
                    if (transform)
                    {
                        var newController = Instantiate(ChallengeRunLoadable.modifierListPrefab, transform).GetComponent<ChallengeRunModifierListPanelController>();
                        newController.hud = hud;
                        controller = newController;
                    }
                }
                else
                {
                    Destroy(controller.gameObject);
                    controller = null;
                }
            }
            return controller;
        }
    }
}
