using MysticsRisky2Utils.ContentManagement;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChallengeMode
{
    public class PetGame : BaseLoadableAsset
    {
        public static GameObject gameSystemPrefab;
        public static GameObject gameSystemInstance;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
        }

        public override void Load()
        {
            gameSystemPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/GrovetenderTender/PetGameSystem.prefab");
            var gameController = gameSystemPrefab.transform.Find("PetGameCanvas/GameController").gameObject.AddComponent<ChallengeModePetGameController>();

            gameController.mainGameHolder = gameController.transform.Find("MainGame").gameObject;
            gameController.headpatHolder = gameController.mainGameHolder.transform.Find("HeadpatAnimation").gameObject;
            gameController.loadingHolder = gameController.transform.Find("LoadingScreen").gameObject;
            gameController.interactiveCanvasHolder = gameSystemPrefab.transform.Find("InteractiveCanvas").gameObject;
            gameController.interactiveCanvasButtonHolder = gameSystemPrefab.transform.Find("InteractiveCanvas/MainGame/Buttons").gameObject;
            gameController.meshHolder = gameSystemPrefab.transform.Find("MeshRenderingSystem/mdlMiniGameConsole").gameObject;
            gameController.screenOffHolder = gameSystemPrefab.transform.Find("MeshRenderingSystem/mdlMiniGameConsole/ScreenOff").gameObject;
            gameController.petAnimator = gameController.mainGameHolder.transform.Find("Pet").GetComponent<Animator>();
            gameController.petBaseAnim = ChallengeModePlugin.AssetBundle.LoadAsset<RuntimeAnimatorController>("Assets/Mods/ChallengeMode/GrovetenderTender/Textures/texPet_0.controller");
            gameController.petSleepAnim = ChallengeModePlugin.AssetBundle.LoadAsset<AnimatorOverrideController>("Assets/Mods/ChallengeMode/GrovetenderTender/Textures/AnimatorOverrides/PetSleep.overrideController");
            gameController.levelText = gameController.mainGameHolder.transform.Find("HUD/LevelText").GetComponent<TextMeshProUGUI>();
            gameController.foodText = gameController.mainGameHolder.transform.Find("HUD/FoodText").GetComponent<TextMeshProUGUI>();
            gameController.expBarFilling = gameController.mainGameHolder.transform.Find("HUD/ExpBar/Filling").GetComponent<Image>();
            gameController.happinessBarFilling = gameController.mainGameHolder.transform.Find("HUD/Stats/Happiness/Bar/Filling").GetComponent<Image>();
            gameController.wakefulnessBarFilling = gameController.mainGameHolder.transform.Find("HUD/Stats/Wakefulness/Bar/Filling").GetComponent<Image>();
            gameController.loadingBarFilling = gameController.loadingHolder.transform.Find("ProgressBar/Filling").GetComponent<Image>();
            gameController.displayMaterial = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/GrovetenderTender/matPetGameRender.mat");
            gameController.consoleTransform = gameSystemPrefab.transform.Find("MeshRenderingSystem/mdlMiniGameConsole");

            void HGifyButton(Button oldButton)
            {
                var buttonObject = oldButton.gameObject;
                Object.DestroyImmediate(oldButton);

                var newButton = buttonObject.AddComponent<MPButton>();
                newButton.transition = Selectable.Transition.None;
                newButton.interactable = true;
                newButton.allowAllEventSystems = true;
            }
            HGifyButton(gameController.interactiveCanvasHolder.transform.Find("MainGame/Buttons/Up").GetComponent<Button>());
            HGifyButton(gameController.interactiveCanvasHolder.transform.Find("MainGame/Buttons/Down").GetComponent<Button>());
            HGifyButton(gameController.interactiveCanvasHolder.transform.Find("MainGame/Buttons/OK").GetComponent<Button>());
            HGifyButton(gameController.interactiveCanvasHolder.transform.Find("MainGame/Buttons/Palette").GetComponent<Button>());
            HGifyButton(gameController.interactiveCanvasHolder.transform.Find("MainGame/Buttons/On Off").GetComponent<Button>());

            gameSystemPrefab.transform.Find("PetGameCamera").position = new Vector3(23042f, -29341f, 44953f);
            gameSystemPrefab.transform.Find("MeshRenderingSystem").position = new Vector3(23042f + 50f, -29341f, 44953f);

            asset = gameSystemPrefab;
        }

        public static void Activate()
        {
            gameSystemInstance = Object.Instantiate(gameSystemPrefab);
        }

        public static void Deactivate()
        {
            if (gameSystemInstance)
                Object.Destroy(gameSystemInstance);
        }

        public class ChallengeModePetGameController : MonoBehaviour
        {
            public int level = 1;
            public float exp = 0;
            public float expToLevelUp = 5;
            public int food = 200;
            public float happiness = 0.5f;
            public float wakefulness = 0.5f;

            public GameObject mainGameHolder;
            public GameObject headpatHolder;
            public GameObject loadingHolder;
            public GameObject interactiveCanvasHolder;
            public GameObject interactiveCanvasButtonHolder;
            public GameObject meshHolder;
            public GameObject screenOffHolder;

            public Animator petAnimator;
            public RuntimeAnimatorController petBaseAnim;
            public AnimatorOverrideController petSleepAnim;

            public TextMeshProUGUI levelText;
            public TextMeshProUGUI foodText;

            public Image expBarFilling;
            public Image happinessBarFilling;
            public Image wakefulnessBarFilling;
            public Image loadingBarFilling;

            public float headpatTimer = 0f;
            public float headpatHappinessRecharge = 0.09f;
            public float timeForFullSadness = 172800f;

            public bool sleeping = false;
            public float sleepWakefulnessRecharge = 0.04f;
            public float timeForFullSleepiness = 32400f;

            public float loadingProgress = 0f;
            public float loadingPerSecond = 1f / 3f;

            public struct ButtonInfo
            {
                public System.Action onClick;
                public GameObject objectToBlink;
            }
            public List<ButtonInfo> inGameButtons;
            public int currentButtonIndex = 0;
            public float buttonBlinkTimer = 0f;
            public float buttonBlinkInterval = 0.5f;
            public bool canUseButtons = true;

            public static List<string> paletteNames = new List<string>()
            {
                "Retro", "Virtual", "Pink", "Orange", "Sky", "Toxic", "FlowerField", "Trans", "Acid", "Neo", "Mocha", "Void"
            };
            public int currentPaletteIndex = 0;
            public Material displayMaterial;

            public enum State
            {
                SlideIn,
                Loading,
                Playing,
                SlideOut
            }
            public State currentState;
            public Transform consoleTransform;
            public float stateAge = 0f;
            public AnimationCurve smooth01Curve;

            public void Awake()
            {
                inGameButtons = new List<ButtonInfo>()
                {
                    new ButtonInfo
                    {
                        onClick = OnFeed,
                        objectToBlink = mainGameHolder.transform.Find("HUD/Buttons/Feed/Text (TMP)").gameObject
                    },
                    new ButtonInfo
                    {
                        onClick = OnHeadpat,
                        objectToBlink = mainGameHolder.transform.Find("HUD/Buttons/Pet/Text (TMP)").gameObject
                    },
                    new ButtonInfo
                    {
                        onClick = OnSleep,
                        objectToBlink = mainGameHolder.transform.Find("HUD/Buttons/Sleep/Text (TMP)").gameObject
                    }
                };

                interactiveCanvasButtonHolder.transform.Find("Up").GetComponent<MPButton>().onClick.AddListener(() =>
                {
                    ChangeCurrentButtonIndex(currentButtonIndex - 1);
                });
                interactiveCanvasButtonHolder.transform.Find("Down").GetComponent<MPButton>().onClick.AddListener(() =>
                {
                    ChangeCurrentButtonIndex(currentButtonIndex + 1);
                });
                interactiveCanvasButtonHolder.transform.Find("OK").GetComponent<MPButton>().onClick.AddListener(ClickCurrentButton);
                interactiveCanvasButtonHolder.transform.Find("Palette").GetComponent<MPButton>().onClick.AddListener(SwapPalette);
                interactiveCanvasButtonHolder.transform.Find("On Off").GetComponent<MPButton>().onClick.AddListener(OnQuit);

                level = ChallengeModeConfig.petLevel.Value;
                exp = ChallengeModeConfig.petExp.Value;
                food = ChallengeModeConfig.petFood.Value;

                var lastLoginTime = ChallengeModeConfig.petLastLoginTime.Value;
                var currentLoginTime = GetCurrentLoginTime();
                var timeSinceLastLogin = 0f;
                if (lastLoginTime != 0f)
                {
                    timeSinceLastLogin = currentLoginTime - lastLoginTime;
                }
                
                happiness = Mathf.Clamp01(ChallengeModeConfig.petHappiness.Value - timeSinceLastLogin / timeForFullSadness);
                wakefulness = Mathf.Clamp01(ChallengeModeConfig.petWakefulness.Value - timeSinceLastLogin / timeForFullSleepiness);

                currentPaletteIndex = paletteNames.IndexOf(ChallengeModeConfig.petPalette.Value);
                if (currentPaletteIndex == -1) currentPaletteIndex = 0;
                UpdateMaterialPalette();

                smooth01Curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

                mainGameHolder.transform.Find("HUD/Buttons/Feed/Text (TMP)").GetComponent<TextMeshProUGUI>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_BUTTON_FEED");
                mainGameHolder.transform.Find("HUD/Buttons/Pet/Text (TMP)").GetComponent<TextMeshProUGUI>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_BUTTON_PET");
                mainGameHolder.transform.Find("HUD/Buttons/Sleep/Text (TMP)").GetComponent<TextMeshProUGUI>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_BUTTON_SLEEP");
                loadingHolder.transform.Find("GameTitle").GetComponent<TextMeshProUGUI>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_TITLE_ONSCREEN");
                meshHolder.transform.Find("OK Button/Text").GetComponent<TextMeshPro>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_CONSOLE_BUTTON_CONFIRM");
                meshHolder.transform.Find("Palette Button/Text").GetComponent<TextMeshPro>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_CONSOLE_BUTTON_PALETTESWAP");
                meshHolder.transform.Find("On Off Button/Text").GetComponent<TextMeshPro>().text = Language.currentLanguage.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_CONSOLE_BUTTON_POWER");
            }

            public void Start()
            {
                currentState = State.SlideIn;
                screenOffHolder.SetActive(true);
                foreach (var button in interactiveCanvasButtonHolder.GetComponentsInChildren<MPButton>())
                    button.interactable = false;

                UpdateStatVisuals();
                UpdateLevelData();
            }

            public void Update()
            {
                stateAge += Time.deltaTime;
                var t = 0f;
                switch (currentState)
                {
                    case State.SlideIn:
                        t = Mathf.Clamp01(stateAge / 2f);
                        consoleTransform.transform.localPosition = new Vector3(0f, -10f + 10f * smooth01Curve.Evaluate(t), 11.46f);
                        consoleTransform.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f - 360f + 360f * smooth01Curve.Evaluate(t), 0f));
                        if (t >= 1f)
                        {
                            screenOffHolder.SetActive(false);
                            loadingHolder.SetActive(true);
                            mainGameHolder.SetActive(false);

                            stateAge = 0f;
                            currentState = State.Loading;
                        }
                        break;
                    case State.Loading:
                        loadingProgress += loadingPerSecond * Time.deltaTime;
                        loadingBarFilling.fillAmount = loadingProgress;
                        if (loadingProgress >= 1f)
                        {
                            loadingHolder.SetActive(false);
                            mainGameHolder.SetActive(true);
                            canUseButtons = true;
                            foreach (var button in interactiveCanvasButtonHolder.GetComponentsInChildren<MPButton>())
                                button.interactable = true;

                            stateAge = 0f;
                            currentState = State.Playing;
                        }
                        break;
                    case State.Playing:
                        if (headpatTimer > 0)
                        {
                            headpatTimer -= Time.deltaTime;
                            happiness = Mathf.Clamp01(happiness + headpatHappinessRecharge * Time.deltaTime);
                            UpdateStatVisuals();
                            if (headpatTimer <= 0)
                            {
                                headpatHolder.SetActive(false);
                            }
                        }
                        if (sleeping)
                        {
                            wakefulness = Mathf.Clamp01(wakefulness + sleepWakefulnessRecharge * Time.deltaTime);
                            UpdateStatVisuals();
                            if (wakefulness >= 1)
                            {
                                sleeping = false;
                                petAnimator.runtimeAnimatorController = petBaseAnim;
                            }
                        }

                        buttonBlinkTimer += Time.deltaTime;
                        if (buttonBlinkTimer >= buttonBlinkInterval)
                        {
                            buttonBlinkTimer -= buttonBlinkInterval;
                            if (currentButtonIndex >= 0 && currentButtonIndex < inGameButtons.Count)
                            {
                                var objectToBlink = inGameButtons[currentButtonIndex].objectToBlink;
                                if (objectToBlink)
                                {
                                    objectToBlink.SetActive(!objectToBlink.activeSelf);
                                }
                            }
                        }
                        break;
                    case State.SlideOut:
                        t = Mathf.Clamp01(stateAge / 1f);
                        consoleTransform.transform.localPosition = new Vector3(0f, -10f * smooth01Curve.Evaluate(t), 11.46f);
                        if (t >= 1f)
                        {
                            PetGame.Deactivate();
                        }
                        break;
                }
            }

            public void OnFeed()
            {
                if (food > 0)
                {
                    sleeping = false;
                    headpatTimer = 0.001f;
                    petAnimator.runtimeAnimatorController = petBaseAnim;
                    food--;
                    exp += 1f * ((happiness + wakefulness) / 2f);
                    UpdateLevelData();
                }
            }

            public void OnHeadpat()
            {
                sleeping = false;
                petAnimator.runtimeAnimatorController = petBaseAnim;
                headpatHolder.SetActive(true);
                headpatTimer = 3f;
            }

            public void OnSleep()
            {
                if (wakefulness >= 1) return;
                headpatTimer = 0.001f;
                petAnimator.runtimeAnimatorController = petSleepAnim;
                sleeping = true;
            }

            public void ChangeCurrentButtonIndex(int newButtonIndex)
            {
                if (currentButtonIndex >= 0 && currentButtonIndex < inGameButtons.Count)
                {
                    var objectToBlink = inGameButtons[currentButtonIndex].objectToBlink;
                    if (objectToBlink)
                    {
                        objectToBlink.SetActive(true);
                    }
                }

                currentButtonIndex = newButtonIndex;
                if (currentButtonIndex >= inGameButtons.Count) currentButtonIndex = 0;
                else if (currentButtonIndex < 0) currentButtonIndex = inGameButtons.Count - 1;
            }

            public void ClickCurrentButton()
            {
                if (!canUseButtons) return;
                if (currentButtonIndex >= 0 && currentButtonIndex < inGameButtons.Count && inGameButtons[currentButtonIndex].onClick != null)
                    inGameButtons[currentButtonIndex].onClick();
            }

            public void UpdateMaterialPalette()
            {
                if (displayMaterial)
                {
                    var newRemapTexture = ChallengeModePlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/ChallengeMode/GrovetenderTender/ColourPalettes/texPalette" + paletteNames[currentPaletteIndex] + ".png");
                    if (newRemapTexture) displayMaterial.SetTexture("_RemapTex", newRemapTexture);
                }
            }

            public void SwapPalette()
            {
                currentPaletteIndex++;
                if (currentPaletteIndex >= paletteNames.Count) currentPaletteIndex = 0;
                UpdateMaterialPalette();
            }

            public void OnQuit()
            {
                screenOffHolder.SetActive(true);
                foreach (var button in interactiveCanvasButtonHolder.GetComponentsInChildren<MPButton>())
                    button.interactable = false;
                canUseButtons = false;
                stateAge = 0f;
                currentState = State.SlideOut;
            }

            public void RecalculateExpToLevelUp()
            {
                expToLevelUp = Mathf.Floor(5f + 2f * Mathf.Pow(level - 1, 1.15f));
                UpdateStatVisuals();
            }

            public void OnLevelUp()
            {
                RecalculateExpToLevelUp();
                UpdateStatVisuals();
            }

            public void UpdateStatVisuals()
            {
                levelText.text = Language.currentLanguage.GetLocalizedFormattedStringByToken("CHALLENGEMODE_PETGAME_LEVEL", level.ToString());
                foodText.text = Language.currentLanguage.GetLocalizedFormattedStringByToken("CHALLENGEMODE_PETGAME_FOOD", food.ToString());
                expBarFilling.fillAmount = exp / expToLevelUp;
                happinessBarFilling.fillAmount = happiness;
                wakefulnessBarFilling.fillAmount = wakefulness;
            }

            public void UpdateLevelData()
            {
                RecalculateExpToLevelUp();
                if (exp >= expToLevelUp)
                {
                    exp -= expToLevelUp;
                    level++;
                    OnLevelUp();
                }
            }

            public float GetCurrentLoginTime()
            {
                return (float)(System.DateTime.Now - System.DateTime.MinValue).TotalSeconds;
            }

            public void OnDestroy()
            {
                ChallengeModeConfig.petLevel.Value = level;
                ChallengeModeConfig.petExp.Value = exp;
                ChallengeModeConfig.petFood.Value = food;
                ChallengeModeConfig.petHappiness.Value = happiness;
                ChallengeModeConfig.petWakefulness.Value = wakefulness;
                ChallengeModeConfig.petLastLoginTime.Value = GetCurrentLoginTime();
                ChallengeModeConfig.petPalette.Value = paletteNames[currentPaletteIndex];
            }
        }
    }
}
