using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace ChallengeMode
{
    public static class ChallengeModeUtils
    {
        public static bool CurrentStageNameMatches(string stageName)
        {
            return Stage.instance && Stage.instance.sceneDef != null && Stage.instance.sceneDef.baseSceneName == stageName;
        }

        public static bool CurrentStageHasCommonInteractables()
        {
            if (Stage.instance && Stage.instance.sceneDef != null && Stage.instance.sceneDef.sceneType != SceneType.Stage)
                return false;

            var unusualStages = new List<string>()
            {
                "moon2", "voidstage", "voidraid", "arena"
            };
            foreach (var stageName in unusualStages)
            {
                if (CurrentStageNameMatches(stageName)) return false;
            }

            return true;
        }

        public static bool CurrentStageHasBosses()
        {
            if (Stage.instance && Stage.instance.sceneDef != null)
            {
                if (Stage.instance.sceneDef.sceneType == SceneType.Cutscene ||
                    Stage.instance.sceneDef.sceneType == SceneType.Menu ||
                    Stage.instance.sceneDef.sceneType == SceneType.Invalid)
                return false;
            }

            var unusualStages = new List<string>()
            {
                "bazaar", "arena", "voidstage"
            };
            foreach (var stageName in unusualStages)
            {
                if (CurrentStageNameMatches(stageName)) return false;
            }

            return true;
        }

        public static void RemoveFromArray<T>(ref T[] array, T element)
        {
            var index = System.Array.IndexOf<T>(array, element);
            if (index != -1)
                HG.ArrayUtils.ArrayRemoveAtAndResize(ref array, index);
        }

        public static void MoveNumberTowards(ref float current, float target, float speed)
        {
            if (current < target)
                current = Mathf.Min(current + speed, target);
            else if (current > target)
                current = Mathf.Max(current - speed, target);
        }

        public static bool IsBodyUnderCeiling(CharacterBody body)
        {
            return Physics.Raycast(new Ray(body.corePosition + Vector3.up * body.radius, Vector3.up), 500f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);
        }

        public static bool BodyIsHot(CharacterBody body)
        {
            var isHot = body.HasBuff(RoR2Content.Buffs.OnFire) || body.HasBuff(DLC1Content.Buffs.StrongerBurn);
            if (!isHot)
            {
                var dotController = DotController.FindDotController(body.gameObject);
                if (dotController && dotController.HasDotActive(DotController.DotIndex.Helfire))
                {
                    isHot = true;
                }
            }
            if (!isHot && onGetBodyIsHot != null) isHot = onGetBodyIsHot(body);
            return isHot;
        }
        public static event System.Func<CharacterBody, bool> onGetBodyIsHot;

        public static bool BodyIsCold(CharacterBody body)
        {
            var isCold = body.HasBuff(RoR2Content.Buffs.Slow80) || (body.healthComponent && body.healthComponent.isInFrozenState);
            if (!isCold && onGetBodyIsCold != null) isCold = onGetBodyIsCold(body);
            return isCold;
        }
        public static event System.Func<CharacterBody, bool> onGetBodyIsCold;
    }
}
