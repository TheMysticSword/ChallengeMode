using UnityEngine;
using MysticsRisky2Utils;
using MysticsRisky2Utils.ContentManagement;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Skills;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ChallengeMode
{
    public class EXBossAssets : BaseLoadableAsset
    {
        public static GameObject fightEffectsPrefab;
        public static GameObject fightEffectsInstance;

        public override void Load()
        {
            fightEffectsPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/ChallengeMode/EXBoss/EXBossFightEffects.prefab");

            OnLoad();
            asset = fightEffectsPrefab;
        }

        public static bool fightEffectsActive
        {
            get
            {
                return fightEffectsInstance != null;
            }
            set
            {
                if (fightEffectsInstance != value)
                {
                    if (value)
                    {
                        if (fightEffectsPrefab)
                        {
                            fightEffectsInstance = Object.Instantiate(fightEffectsPrefab);
                            var weatherParticles = fightEffectsInstance.AddComponent<WeatherParticles>();
                            weatherParticles.lockPosition = true;
                            weatherParticles.lockRotation = false;
                        }
                    }
                    else
                    {
                        if (fightEffectsInstance) Object.Destroy(fightEffectsInstance);
                    }
                }
            }
        }
    }
}
