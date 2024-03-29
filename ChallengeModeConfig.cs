using MysticsRisky2Utils;
using RiskOfOptions;
using RoR2;

namespace ChallengeMode
{
    public static class ChallengeModeConfig
    {
        public static ConfigOptions.ConfigurableValue<int> modifiersPerStage = ConfigOptions.ConfigurableValue.CreateInt(
            ChallengeModePlugin.PluginGUID,
            ChallengeModePlugin.PluginName,
            ChallengeModePlugin.config,
            "General",
            "Random Modifiers",
            3,
            0,
            5,
            "Amount of random modifiers to add"
        );

        public static ConfigOptions.ConfigurableValue<float> uniqueModifierChance = ConfigOptions.ConfigurableValue.CreateFloat(
            ChallengeModePlugin.PluginGUID,
            ChallengeModePlugin.PluginName,
            ChallengeModePlugin.config,
            "General",
            "Stage Modifier Chance",
            20f,
            0f,
            100f,
            "Chance for stage-unique modifiers to appear additionally on top of the random modifiers"
        );

        public static ConfigOptions.ConfigurableValue<float> uniqueModifierRareChance = ConfigOptions.ConfigurableValue.CreateFloat(
            ChallengeModePlugin.PluginGUID,
            ChallengeModePlugin.PluginName,
            ChallengeModePlugin.config,
            "General",
            "Stage Modifier Rare Chance",
            5f,
            0f,
            100f,
            "Same as Stage Modifier Chance, but for modifiers that appear very rarely in some other stages (for example, Frostbite can have 20% in Rallypoint Delta, and 5% in Siphoned Forest)"
        );

        public static ConfigOptions.ConfigurableValue<float> specialModifierChance = ConfigOptions.ConfigurableValue.CreateFloat(
            ChallengeModePlugin.PluginGUID,
            ChallengeModePlugin.PluginName,
            ChallengeModePlugin.config,
            "General",
            "Boss Modifier Chance",
            100f,
            0f,
            100f,
            "Chance for final boss modifiers to appear additionally on top of the random modifiers"
        );

        public static ConfigOptions.ConfigurableValue<bool> permanentBrotherEX = ConfigOptions.ConfigurableValue.CreateBool(
            ChallengeModePlugin.PluginGUID,
            ChallengeModePlugin.PluginName,
            ChallengeModePlugin.config,
            "Permanent Toggles",
            "Mithrix EX",
            false,
            "Should the reworked Mithrix fight be active outside of the game mode? (Takes effect only if set before the run starts!)"
        );

        public static ConfigOptions.ConfigurableValue<bool> permanentVoidRaidCrabEX = ConfigOptions.ConfigurableValue.CreateBool(
            ChallengeModePlugin.PluginGUID,
            ChallengeModePlugin.PluginName,
            ChallengeModePlugin.config,
            "Permanent Toggles",
            "Voidling EX",
            false,
            "Should the reworked Voidling fight be active outside of the game mode? (Takes effect only if set before the run starts!)"
        );

        public static BepInEx.Configuration.ConfigEntry<int> petLevel;
        public static BepInEx.Configuration.ConfigEntry<float> petExp;
        public static BepInEx.Configuration.ConfigEntry<int> petFood;
        public static BepInEx.Configuration.ConfigEntry<float> petHappiness;
        public static BepInEx.Configuration.ConfigEntry<float> petWakefulness;
        public static BepInEx.Configuration.ConfigEntry<float> petLastLoginTime;
        public static BepInEx.Configuration.ConfigEntry<string> petPalette;

        public static void Init()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;

            ModSettingsManager.AddOption(new RiskOfOptions.Options.GenericButtonOption("PetGame", "General", "", "Play", PetGame.Activate));
            On.RoR2.Language.GetLocalizedStringByToken += Language_GetLocalizedStringByToken;
            petLevel = ChallengeModePlugin.config.Bind("PetGame", "Level", 1);
            petExp = ChallengeModePlugin.config.Bind("PetGame", "Exp", 0f);
            petFood = ChallengeModePlugin.config.Bind("PetGame", "Food", 0);
            petHappiness = ChallengeModePlugin.config.Bind("PetGame", "Happiness", 0.5f);
            petWakefulness = ChallengeModePlugin.config.Bind("PetGame", "Wakefulness", 0.5f);
            petLastLoginTime = ChallengeModePlugin.config.Bind("PetGame", "LastLoginTime", 0f);
            petPalette = ChallengeModePlugin.config.Bind("PetGame", "Palette", "Retro");
        }

        private static void Run_onRunStartGlobal(Run run)
        {
            var challengeRun = run as ChallengeRun;
            if (challengeRun == null)
            {
                void ProcessPermanentModifier(bool shouldBeActive, string modifierName)
                {
                    if (shouldBeActive && ChallengeRunModifierCatalog.nameToModifier.TryGetValue(modifierName, out var modifier) && !modifier.isActive)
                    {
                        modifier.OnEnable();
                    }
                }
                ProcessPermanentModifier(permanentBrotherEX, "BrotherEX");
                ProcessPermanentModifier(permanentVoidRaidCrabEX, "VoidRaidCrabEX");
            }
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            var challengeRun = run as ChallengeRun;
            if (challengeRun == null)
            {
                void ProcessPermanentModifier(string modifierName)
                {
                    if (ChallengeRunModifierCatalog.nameToModifier.TryGetValue(modifierName, out var modifier) && modifier.isActive)
                    {
                        modifier.OnDisable();
                    }
                }
                ProcessPermanentModifier("BrotherEX");
                ProcessPermanentModifier("VoidRaidCrabEX");
            }
        }

        private static string Language_GetLocalizedStringByToken(On.RoR2.Language.orig_GetLocalizedStringByToken orig, RoR2.Language self, string token)
        {
            if (token == "RISK_OF_OPTIONS.COM.THEMYSTICSWORD.CHALLENGEMODE.GENERAL.PETGAME.GENERIC_BUTTON.NAME")
                return self.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_TITLE");
            if (token == "RISK_OF_OPTIONS.COM.THEMYSTICSWORD.CHALLENGEMODE.GENERAL.PETGAME.GENERIC_BUTTON.SUB_BUTTON.NAME")
                return self.GetLocalizedStringByToken("CHALLENGEMODE_PETGAME_PLAY");
            return orig(self, token);
        }
    }
}