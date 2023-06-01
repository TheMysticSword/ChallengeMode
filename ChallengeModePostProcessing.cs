using RoR2;
using UnityEngine;

namespace ChallengeMode.PostProcessing
{
    public class ChallengeModePostProcessing : MonoBehaviour
    {
        public CameraRigController cameraRigController;
        public Camera camera;

        public static Material frostbiteMaterialPrefab;
        public float frostbiteIntensity = 0f;
        public Material frostbiteMaterialInstance;

        public static Material rainyMaterialPrefab;
        public float rainyIntensity = 0f;
        public Material rainyMaterialInstance;

        public static Material heatMaterialPrefab;
        public float heatIntensity = 0f;
        public Material heatMaterialInstance;

        public static Material vhsRewindMaterialPrefab;
        public float vhsRewindIntensity = 0f;
        public Material vhsRewindMaterialInstance;

        public static Material voidRaidCrabScreamMaterialPrefab;
        public float voidRaidCrabScreamIntensity = 0f;
        public Material voidRaidCrabScreamMaterialInstance;

        public bool distortionEnabled = false;

        public static float voidRaidCrabScreamIntensityTarget = 0f;
        public static Vector3 voidRaidCrabWorldPosition = Vector3.one * 0.5f;
        public Vector2 voidRaidCrabCameraPosition = Vector2.one * 0.5f;
        public Vector2 voidRaidCrabCameraPositionTarget = Vector2.one * 0.5f;
        public Vector2 voidRaidCrabCameraVelocity = Vector2.zero;

        public void Awake()
        {
            frostbiteMaterialInstance = Instantiate(frostbiteMaterialPrefab);
            rainyMaterialInstance = Instantiate(rainyMaterialPrefab);
            heatMaterialInstance = Instantiate(heatMaterialPrefab);
            vhsRewindMaterialInstance = Instantiate(vhsRewindMaterialPrefab);
            voidRaidCrabScreamMaterialInstance = Instantiate(voidRaidCrabScreamMaterialPrefab);

            var sceneCamera = GetComponent<SceneCamera>();
            if (sceneCamera)
            {
                cameraRigController = sceneCamera.cameraRigController;
            }
        }

        public void Update()
        {
            if (cameraRigController && cameraRigController.targetBody)
            {
                var frostbiteIntensityTarget = (float)cameraRigController.targetBody.GetBuffCount(ChallengeModeContent.Buffs.ChallengeMode_Frostbite) / (float)Modifiers.Unique.Frostbite.maxTimeInCold;
                ChallengeModeUtils.MoveNumberTowards(
                    ref frostbiteIntensity,
                    frostbiteIntensityTarget,
                    (frostbiteIntensity < frostbiteIntensityTarget ? 1f : 10f) * Time.deltaTime / (float)Modifiers.Unique.Frostbite.maxTimeInCold
                );

                ChallengeModeUtils.MoveNumberTowards(
                    ref rainyIntensity,
                    (ChallengeRunModifierCatalog.nameToModifier["AcidRain"].isActive && !ChallengeModeUtils.IsBodyUnderCeiling(cameraRigController.targetBody)) ? 1f : 0f,
                    0.5f * Time.deltaTime
                );

                var heatIntensityTarget = 0f;
                if (ChallengeRunModifierCatalog.nameToModifier["HotSand"].isActive)
                    heatIntensityTarget = Mathf.Clamp01((float)cameraRigController.targetBody.GetBuffCount(RoR2Content.Buffs.Overheat) / (float)Modifiers.Unique.HotSand.overheatThreshold);
                ChallengeModeUtils.MoveNumberTowards(
                    ref heatIntensity,
                    heatIntensityTarget,
                    1f * Time.deltaTime
                );

                if (cameraRigController.targetBody.HasBuff(ChallengeModeContent.Buffs.ChallengeMode_CommsJammedVisuals))
                    vhsRewindIntensity = 1f;
                else
                    vhsRewindIntensity = 0f;

                ChallengeModeUtils.MoveNumberTowards(
                    ref voidRaidCrabScreamIntensity,
                    voidRaidCrabScreamIntensityTarget,
                    5f * Time.deltaTime
                );
            }

            if (!camera)
            {
                camera = GetComponent<Camera>();
            }
            else
            {
                if (camera.depthTextureMode != DepthTextureMode.DepthNormals)
                    camera.depthTextureMode = DepthTextureMode.DepthNormals;
            }

            distortionEnabled = !SettingsConVars.PpScreenDistortionConVar.settings || !SettingsConVars.PpScreenDistortionConVar.settings.active;
        }

        public void LateUpdate()
        {
            if (voidRaidCrabScreamIntensity > 0 && camera)
            {
                var voidRaidCrabCameraPosition3D = camera.WorldToScreenPoint(voidRaidCrabWorldPosition);

                var targetBehindCamera = voidRaidCrabCameraPosition3D.z <= 0f;
                voidRaidCrabCameraPositionTarget.x = voidRaidCrabCameraPosition3D.x;
                voidRaidCrabCameraPositionTarget.y = voidRaidCrabCameraPosition3D.y;
                // voidRaidCrabCameraPositionTarget *= targetBehindCamera ? -1f : 1f;
                if (!targetBehindCamera)
                {
                    voidRaidCrabCameraPositionTarget.x = Mathf.Clamp01(voidRaidCrabCameraPositionTarget.x / camera.pixelWidth);
                    voidRaidCrabCameraPositionTarget.y = Mathf.Clamp01(voidRaidCrabCameraPositionTarget.y / camera.pixelHeight);
                }
                else
                {
                    voidRaidCrabCameraPositionTarget.x = 0.5f;
                    voidRaidCrabCameraPositionTarget.y = 0.5f;
                }
                voidRaidCrabCameraPosition = Vector2.SmoothDamp(voidRaidCrabCameraPosition, voidRaidCrabCameraPositionTarget, ref voidRaidCrabCameraVelocity, 0.1f);
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (frostbiteIntensity <= 0 &&
                rainyIntensity <= 0 &&
                heatIntensity <= 0 &&
                vhsRewindIntensity <= 0 &&
                voidRaidCrabScreamIntensity <= 0)
            {
                Graphics.Blit(source, destination);
            }
            else
            {
                if (rainyIntensity > 0 && rainyMaterialInstance)
                {
                    rainyMaterialInstance.SetFloat("_Intensity", rainyIntensity);
                    rainyMaterialInstance.SetFloat("_DisplacementOn", distortionEnabled ? 1 : 0);
                    Graphics.Blit(source, destination, rainyMaterialInstance);
                }
                if (heatIntensity > 0 && heatMaterialInstance)
                {
                    heatMaterialInstance.SetFloat("_Intensity", heatIntensity * heatIntensity);
                    heatMaterialInstance.SetFloat("_DisplacementOn", distortionEnabled ? 1 : 0);
                    Graphics.Blit(source, destination, heatMaterialInstance);
                }
                if (frostbiteIntensity > 0 && frostbiteMaterialInstance)
                {
                    frostbiteMaterialInstance.SetFloat("_Intensity", frostbiteIntensity * frostbiteIntensity);
                    frostbiteMaterialInstance.SetFloat("_DisplacementOn", distortionEnabled ? 1 : 0);
                    Graphics.Blit(source, destination, frostbiteMaterialInstance);
                }
                if (vhsRewindIntensity > 0 && vhsRewindMaterialInstance)
                {
                    vhsRewindMaterialInstance.SetFloat("_Intensity", vhsRewindIntensity);
                    vhsRewindMaterialInstance.SetFloat("_DisplacementOn", distortionEnabled ? 1 : 0);
                    Graphics.Blit(source, destination, vhsRewindMaterialInstance);
                }
                if (voidRaidCrabScreamIntensity > 0 && voidRaidCrabScreamMaterialInstance)
                {
                    voidRaidCrabScreamMaterialInstance.SetFloat("_Intensity", voidRaidCrabScreamIntensity);
                    voidRaidCrabScreamMaterialInstance.SetFloat("_DisplacementOn", distortionEnabled ? 1 : 0);
                    voidRaidCrabScreamMaterialInstance.SetFloat("_CenterX", voidRaidCrabCameraPosition.x);
                    voidRaidCrabScreamMaterialInstance.SetFloat("_CenterY", voidRaidCrabCameraPosition.y);
                    Graphics.Blit(source, destination, voidRaidCrabScreamMaterialInstance);
                }
            }
        }

        public void OnDestroy()
        {
            if (frostbiteMaterialInstance) Destroy(frostbiteMaterialInstance);
            if (rainyMaterialInstance) Destroy(rainyMaterialInstance);
            if (heatMaterialInstance) Destroy(heatMaterialInstance);
            if (vhsRewindMaterialInstance) Destroy(vhsRewindMaterialInstance);
            if (voidRaidCrabScreamMaterialInstance) Destroy(voidRaidCrabScreamMaterialInstance);
        }

        internal static void Init()
        {
            On.RoR2.SceneCamera.Awake += SceneCamera_Awake;

            frostbiteMaterialPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/Modifiers/Frostbite/matFrostbitePostProcessing.mat");
            rainyMaterialPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/Common/RainyFilter/matRainyFilter.mat");
            heatMaterialPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/Modifiers/HotSand/matHeatFilter.mat");
            vhsRewindMaterialPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/Common/VHSRewindFilter/matVHSRewind.mat");
            voidRaidCrabScreamMaterialPrefab = ChallengeModePlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/ChallengeMode/Modifiers/VoidRaidCrabEX/matVoidRaidCrabScreamFilter.mat");
        }

        private static void SceneCamera_Awake(On.RoR2.SceneCamera.orig_Awake orig, SceneCamera self)
        {
            orig(self);
            self.gameObject.AddComponent<ChallengeModePostProcessing>();
        }
    }
}