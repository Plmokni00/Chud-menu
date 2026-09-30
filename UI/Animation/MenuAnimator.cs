using Chud.Diagnostics;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
namespace Chud.UI
{
    public sealed class MenuAnimItem
    {
        public Transform Body;
        public Transform Visual;
        public Transform Label;
        public Transform Outline;
        public Vector3 BodyRest = Vector3.one;
        public Vector3 VisualRest = Vector3.one;
        public Vector3 OutlineRest = Vector3.one;
        public Func<float, Vector3> LabelScale;
    }

    public sealed class MenuAnimPlan
    {
        public Vector3 TargetScale = Vector3.one;
        public Vector3 FoldedScale = Vector3.one;
        public readonly List<MenuAnimItem> Items = new List<MenuAnimItem>(8);
        public MenuAnimItem Disconnect;
    }

    public static class MenuAnimator
    {
        private const float BookDuration = 0.18f;
        private const float ButtonDuration = 0.08f;
        private const float NavDuration = 0.04f;
        private const float Stagger = 0.03f;
        private const float CloseDuration = 0.3f;

        public static IEnumerator Open(MenuAnimPlan plan)
        {
            if (plan == null || WristMenu.Menu == (Object)null)
            {
                yield break;
            }

            if (!WristMenu.AnimationsEnabled)
            {
                WristMenu.Menu.transform.localScale = plan.TargetScale;
                yield break;
            }

            MenuAnimItem[] ordered = OrderFrontToBack(plan.Items);
            SetAll(ordered, plan.Disconnect, Vector3.zero, false);
            WristMenu.AnimatorOwnsScale = true;

            float elapsed = 0f;
            while (elapsed < BookDuration)
            {
                if (WristMenu.Menu == (Object)null)
                {
                    WristMenu.AnimatorOwnsScale = false;
                    yield break;
                }

                float t = Mathf.Clamp01(elapsed / BookDuration);
                WristMenu.Menu.transform.localScale = Vector3.Lerp(plan.FoldedScale, plan.TargetScale, SmoothStep(t));
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (WristMenu.Menu != (Object)null)
            {
                WristMenu.Menu.transform.localScale = plan.TargetScale;
            }

            WristMenu.AnimatorOwnsScale = false;

            for (int i = 0; i < ordered.Length; i++)
            {
                if (WristMenu.Menu == (Object)null || WristMenu.Instance == (Object)null)
                {
                    yield break;
                }

                WristMenu.Instance.StartCoroutine(BuildItem(ordered[i], ButtonDuration));
                yield return new WaitForSeconds(Stagger);
            }

            yield return new WaitForSeconds(ButtonDuration);

            if (WristMenu.Instance != (Object)null)
            {
                WristMenu.Instance.StartCoroutine(BuildItem(plan.Disconnect, NavDuration));
            }

            yield return new WaitForSeconds(NavDuration);
        }

        public static IEnumerator Close()
        {
            if (WristMenu.Menu == (Object)null || WristMenu.Close)
            {
                yield break;
            }

            if (!WristMenu.AnimationsEnabled)
            {
                WristMenu.DestroyMenu();
                yield break;
            }

            WristMenu.Close = true;
            WristMenu.AnimatorOwnsScale = true;

            Vector3 startScale = WristMenu.Menu.transform.localScale;
            var targetScale = Vector3.zero;

            float elapsed = 0f;
            while (elapsed < CloseDuration)
            {
                if (WristMenu.Menu == (Object)null)
                {
                    WristMenu.Close = false;
                    WristMenu.AnimatorOwnsScale = false;
                    yield break;
                }

                float t = elapsed / CloseDuration;
                const float s = 1.70158f;
                float bounce = t * t * ((s + 1f) * t - s);
                WristMenu.Menu.transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, bounce);
                elapsed += Time.deltaTime;
                yield return null;
            }

            WristMenu.Close = false;
            WristMenu.AnimatorOwnsScale = false;
            WristMenu.DestroyMenu();
        }

        public static void SetItem(MenuAnimItem item, Vector3 scale, bool showText)
        {
            if (item == null)
            {
                return;
            }

            if (item.Body != (Object)null)
            {
                item.Body.localScale = scale;
            }

            if (item.Visual != (Object)null)
            {
                item.Visual.localScale = scale;
            }

            if (item.Outline != (Object)null)
            {
                item.Outline.localScale = scale;
            }

            if (item.Label != (Object)null && item.LabelScale != null)
            {
                item.Label.localScale = item.LabelScale(showText ? 1f : 0f);
            }
        }

        private static IEnumerator BuildItem(MenuAnimItem item, float duration)
        {
            if (item == null)
            {
                yield break;
            }

            Vector3 bodyTarget = item.BodyRest;
            Vector3 visualTarget = item.VisualRest;
            Vector3 outlineTarget = item.OutlineRest;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (WristMenu.Menu == (Object)null)
                {
                    yield break;
                }

                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - (1f - t) * (1f - t);

                if (item.Body != (Object)null)
                {
                    item.Body.localScale = Vector3.Lerp(Vector3.zero, bodyTarget, eased);
                }

                if (item.Visual != (Object)null)
                {
                    item.Visual.localScale = Vector3.Lerp(Vector3.zero, visualTarget, eased);
                }

                if (item.Outline != (Object)null)
                {
                    item.Outline.localScale = Vector3.Lerp(Vector3.zero, outlineTarget, eased);
                }

                if (item.Label != (Object)null && item.LabelScale != null)
                {
                    item.Label.localScale = item.LabelScale(eased);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (item.Body != (Object)null)
            {
                item.Body.localScale = bodyTarget;
            }

            if (item.Visual != (Object)null)
            {
                item.Visual.localScale = visualTarget;
            }

            if (item.Outline != (Object)null)
            {
                item.Outline.localScale = outlineTarget;
            }

            if (item.Label != (Object)null && item.LabelScale != null)
            {
                item.Label.localScale = item.LabelScale(1f);
            }
        }

        private static void SetAll(MenuAnimItem[] items, MenuAnimItem disconnect, Vector3 scale, bool showText)
        {
            for (int i = 0; i < items.Length; i++)
            {
                SetItem(items[i], scale, showText);
            }

            SetItem(disconnect, scale, showText);
        }

        private static MenuAnimItem[] OrderFrontToBack(List<MenuAnimItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return Array.Empty<MenuAnimItem>();
            }

            var ordered = new MenuAnimItem[items.Count];
            items.CopyTo(ordered);

            Array.Sort(ordered, (a, b) =>
            {
                float az = a.Body != (Object)null ? a.Body.localPosition.z : 0f;
                float bz = b.Body != (Object)null ? b.Body.localPosition.z : 0f;
                return bz.CompareTo(az);
            });

            return ordered;
        }

        private static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}