using MysticsRisky2Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace ChallengeMode
{
    public class ChallengeRunModifier
    {
        public virtual string nameToken => "";
        public virtual string descriptionToken => "";
        public virtual bool isAdditional => false;

        internal int modifierIndex = -1;

        public bool isActive = false;

        public virtual void OnEnable()
        {
            isActive = true;
        }

        public virtual void OnDisable()
        {
            isActive = false;
        }

        public virtual bool IsAvailable()
        {
            return true;
        }

        internal ConfigOptions.ConfigurableValue<bool> configEnableOption;
        public bool isEnabledInConfig
        {
            get
            {
                if (configEnableOption != null) return configEnableOption;
                return true;
            }
        }
    }

    public static class ChallengeRunModifierCatalog
    {
        public static List<ChallengeRunModifier> allModifiers = new List<ChallengeRunModifier>();

        public static Dictionary<int, ChallengeRunModifier> indexToModifier = new Dictionary<int, ChallengeRunModifier>();
        public static Dictionary<string, ChallengeRunModifier> nameToModifier = new Dictionary<string, ChallengeRunModifier>();

        public static void Init()
        {
            allModifiers.Add(new Modifiers.AlwaysSlippery());
            allModifiers.Add(new Modifiers.UtilityCooldown());
            allModifiers.Add(new Modifiers.ChestFailChance());
            allModifiers.Add(new Modifiers.NoOSP());
            allModifiers.Add(new Modifiers.PrimaryStamina());
            allModifiers.Add(new Modifiers.PrinterBug());
            allModifiers.Add(new Modifiers.LowHPStress());
            allModifiers.Add(new Modifiers.PerfectedOnPlanet());
            allModifiers.Add(new Modifiers.UselessSteak());
            allModifiers.Add(new Modifiers.MalachiteSpikeOnKill());
            // 10
            allModifiers.Add(new Modifiers.FireBackfire());
            allModifiers.Add(new Modifiers.BossFocus());
            allModifiers.Add(new Modifiers.DoubleJumpHurts());
            allModifiers.Add(new Modifiers.CurseOnHeavyHit());
            allModifiers.Add(new Modifiers.BlocksForHealth());
            allModifiers.Add(new Modifiers.ExpensivePurchasables());
            allModifiers.Add(new Modifiers.MissChance());
            allModifiers.Add(new Modifiers.ReducedVision());
            allModifiers.Add(new Modifiers.BurnChance());
            allModifiers.Add(new Modifiers.AllChestsCloaked());
            // 20
            allModifiers.Add(new Modifiers.LoseGoldOverTime());
            allModifiers.Add(new Modifiers.BombDrop());
            allModifiers.Add(new Modifiers.NoGoingBack());
            allModifiers.Add(new Modifiers.BigShot());
            allModifiers.Add(new Modifiers.DroppingItems());
            allModifiers.Add(new Modifiers.RandomMeteors());
            allModifiers.Add(new Modifiers.DronePurgatory());
            allModifiers.Add(new Modifiers.SteakReplacesRandomItems());
            allModifiers.Add(new Modifiers.PurchasesInflictSlow());
            allModifiers.Add(new Modifiers.RerollItems());
            // 30
            allModifiers.Add(new Modifiers.IncludeVoidReavers());
            allModifiers.Add(new Modifiers.HalfHealingToShields());
            allModifiers.Add(new Modifiers.DefenseDamage());
            allModifiers.Add(new Modifiers.EnemiesHaveShield());
            allModifiers.Add(new Modifiers.BossRevive());
            allModifiers.Add(new Modifiers.EquipmentCDRandomized());
            allModifiers.Add(new Modifiers.BackpackWeight());
            allModifiers.Add(new Modifiers.KillRemorse());
            allModifiers.Add(new Modifiers.HoldoutShrink());
            allModifiers.Add(new Modifiers.TurnOffMyPainInhibitors());
            // 40
            allModifiers.Add(new Modifiers.BrokenSpine());
            allModifiers.Add(new Modifiers.DebuffInTPRange());
            allModifiers.Add(new Modifiers.NoCrits());
            allModifiers.Add(new Modifiers.IncreasedKnockback());
            allModifiers.Add(new Modifiers.EnemyMissileOnHit());
            allModifiers.Add(new Modifiers.FlutterJumps());
            allModifiers.Add(new Modifiers.FasterBarrierDecay());
            allModifiers.Add(new Modifiers.EnemyLeech());
            allModifiers.Add(new Modifiers.ProcLimit());
            allModifiers.Add(new Modifiers.BossHalfHPArmor());
            // 50

            allModifiers.Add(new Modifiers.Unique.Frostbite());
            allModifiers.Add(new Modifiers.Unique.AcidRain());
            allModifiers.Add(new Modifiers.Unique.MountainWinds());
            allModifiers.Add(new Modifiers.Unique.HotSand());
            allModifiers.Add(new Modifiers.Unique.CommsJam());
            allModifiers.Add(new Modifiers.Unique.Thunder());

            allModifiers.Add(new Modifiers.Special.BrotherEX());
            allModifiers.Add(new Modifiers.Special.VoidRaidCrabEX());

            if (onCollectModifiers != null) onCollectModifiers(allModifiers);

            int i = 0;
            foreach (var modifier in allModifiers)
            {
                modifier.modifierIndex = i;
                indexToModifier[i] = modifier;
                nameToModifier[modifier.GetType().Name] = modifier;

                i++;
            }

            RoR2Application.onLoad += () =>
            {
                var richTextRegex = new System.Text.RegularExpressions.Regex(@"<[^>]*>");
                var forbiddenChars = new List<char>() { '=', '\n', '\t', '\\', '"', '\'', '[', ']' };
                string GetFilteredString(string inputString)
                {
                    inputString = richTextRegex.Replace(inputString, string.Empty);

                    foreach (var forbiddenChar in forbiddenChars)
                        inputString = inputString.Replace(forbiddenChar, ' ');
                    
                    return inputString;
                }

                foreach (var modifier in allModifiers)
                {
                    modifier.configEnableOption = ConfigOptions.ConfigurableValue.CreateBool(
                        ChallengeModePlugin.PluginGUID,
                        ChallengeModePlugin.PluginName,
                        ChallengeModePlugin.config,
                        "Enabled Modifiers",
                        GetFilteredString(Language.english.GetLocalizedStringByToken(modifier.nameToken)),
                        true,
                        GetFilteredString(Language.english.GetLocalizedStringByToken(modifier.descriptionToken))
                    );
                }
            };
        }

        public static event System.Action<List<ChallengeRunModifier>> onCollectModifiers;
    }
}
