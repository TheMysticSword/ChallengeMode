using TMPro;
using UnityEngine;

namespace ChallengeMode
{
    public static class TMProEffects
    {
        public static void Init()
        {
            On.RoR2.UI.ChatBox.Start += ChatBox_Start;
            On.RoR2.UI.HGTextMeshProUGUI.Awake += HGTextMeshProUGUI_Awake;
        }

        private static void ChatBox_Start(On.RoR2.UI.ChatBox.orig_Start orig, RoR2.UI.ChatBox self)
        {
            orig(self);
            self.messagesText.textComponent.gameObject.AddComponent<ChallengeModeTextEffects>();
        }

        private static void HGTextMeshProUGUI_Awake(On.RoR2.UI.HGTextMeshProUGUI.orig_Awake orig, RoR2.UI.HGTextMeshProUGUI self)
        {
            orig(self);
            var component = self.GetComponent<ChallengeModeTextEffects>();
            if (!component)
            {
                component = self.gameObject.AddComponent<ChallengeModeTextEffects>();
                component.textComponent = self;
            }
        }

        public class ChallengeModeTextEffects : MonoBehaviour
        {
            public TMP_Text textComponent;
            public bool textChanged;
            public TMP_MeshInfo[] cachedMeshInfo;

            public float updateTimer = 0f;
            public float updateFrequency = 0.016f;

            public float currentTime = 0f;

            public void Awake()
            {
                textComponent = GetComponent<TMP_Text>();
                textChanged = true;
            }

            public void Start()
            {
                if (textComponent && textComponent.isActiveAndEnabled)
                {
                    textComponent.ForceMeshUpdate();
                }
            }

            public void OnEnable()
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
            }

            public void OnDisable()
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
            }

            public void ON_TEXT_CHANGED(Object obj)
            {
                if (obj == textComponent)
                    textChanged = true;
            }

            public void Update()
            {
                currentTime += Time.deltaTime;

                updateTimer -= Time.deltaTime;
                while (updateTimer <= 0f)
                {
                    updateTimer += updateFrequency;
                    if (textComponent && textComponent.isActiveAndEnabled)
                    {
                        textComponent.ForceMeshUpdate();

                        var textInfo = textComponent.textInfo;

                        if (textChanged)
                        {
                            textChanged = false;
                            cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                        }

                        var anythingChanged = false;

                        for (var linkIndex = 0; linkIndex < textInfo.linkCount; linkIndex++)
                        {
                            var link = textInfo.linkInfo[linkIndex];
                            if (link.GetLinkID() == "ChallengeModeShaky")
                            {
                                for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
                                {
                                    var charInfo = textInfo.characterInfo[i];
                                    if (!charInfo.isVisible) continue;

                                    anythingChanged = true;

                                    var shakeAmount = 2f;
                                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                                    var shakeOffset = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0f) * charInfo.scale;
                                    for (var j = 0; j <= 3; j++)
                                    {
                                        verts[charInfo.vertexIndex + j] = verts[charInfo.vertexIndex + j] + shakeOffset;
                                    }
                                }
                            }
                            if (link.GetLinkID() == "ChallengeModeRainbow")
                            {
                                for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
                                {
                                    var charInfo = textInfo.characterInfo[i];
                                    if (!charInfo.isVisible) continue;

                                    anythingChanged = true;

                                    var origColors = cachedMeshInfo[charInfo.materialReferenceIndex].colors32;
                                    var destColors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                                    for (var j = 0; j <= 3; j++)
                                    {
                                        destColors[charInfo.vertexIndex + j] = Color.Lerp(origColors[charInfo.vertexIndex + j], Color.HSVToRGB((Time.time * 0.15f + 0.06f * i) % 1f, 1f, 1f), 0.2f);
                                    }
                                }
                            }
                        }

                        if (anythingChanged)
                        {
                            for (var i = 0; i < textInfo.meshInfo.Length; i++)
                            {
                                var meshInfo = textInfo.meshInfo[i];
                                meshInfo.mesh.vertices = meshInfo.vertices;
                                meshInfo.mesh.colors32 = meshInfo.colors32;
                                textComponent.UpdateGeometry(meshInfo.mesh, i);
                                textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                            }
                        }
                    }
                }
            }
        }
    }
}
