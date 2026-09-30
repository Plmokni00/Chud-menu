using Chud.Menu;
using Chud.Rendering;
using Chud.Runtime;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
namespace Chud.UI
{
    internal sealed class ModernLayout
    {
        public const string Id = "modern";

        public const float MenuScaleFactor = 0.95f;

        private const float MenuRadius = 0.1f;
        private const float MenuHeight = 0.3f;
        private const float MenuDepth = 0.4f;
        private const float MenuInitialDepthFactor = 0.95625f;

        public const float PanelThickness = 0.08f;
        public const float PanelFrontOffset = 0.533f;
        public const float PanelX = PanelFrontOffset - PanelThickness * 0.425f;
        public const float PanelWidth = 0.9f;
        public const float PanelHeight = 1f;
        public const float PanelFrontX = 0.56f;
        public const float LabelX = PanelFrontX + 0.035f;

        public const float TitleZ = 0.4f;
        public const float TitleHeight = 0.088f;
        public const float TitleHeightBig = 0.1f;
        public const float TitleWidth = 0.78f;

        public const float StatusZ = 0.462f;
        public const float StatusWidth = 0.72f;
        public const float StatusHeight = 0.026f;

        public const float DisconnectZ = 0.556f;

        public const float FirstButtonZ = 0.275f;
        public const float ButtonPitch = 0.088f;
        public const float ButtonHeight = 0.072f;
        public const float ButtonWidth = 0.76f;
        public const float ButtonDepth = 0.05f;
        public const float ButtonRadius = 0.16f;
        public const int ButtonCornerSegments = 6;

        public const float LabelShiftY = 0f;
        public const float LabelShiftZ = 0.008f;
        public const float ButtonTextWidthRatio = 0.9f;
        public const float ButtonTextHeightRatio = 0.73f;
        public const float NavTextWidthRatio = 0.55f;
        public const float NavTextHeightRatio = 0.55f;
        public const float LabelMaxWidth = ButtonWidth * ButtonTextWidthRatio;
        public const float LabelMaxHeight = ButtonHeight * ButtonTextHeightRatio;

        public const float NavZ = -0.375f;
        public const float NavHeight = 0.1f;
        public const float NavWidth = 0.36f;
        public const float NavOffsetY = 0.205f;
        public const float NavDepth = 0.05f;

        public const float OutlineDepth = 0.01f;
        public const float OutlineOffset = 0.016f;
        public const float OutlineBorder = 0.018f;
        public const float NavOutlineRadius = 0.22f;

        private const float BackgroundMeshRadius = 0.08f;
        private const int BackgroundMeshSegments = 6;
        private const float BackgroundMeshDepth = 0.85f;

        private readonly Dictionary<string, List<Renderer>> _roundedRenderers =
            new Dictionary<string, List<Renderer>>(32);

        private readonly Dictionary<string, Vector3> _restScales = new Dictionary<string, Vector3>(32);

        public ModernTextLayer Text { get; private set; }

        public static Vector3 MenuTargetScale(float scale)
        {
            return new Vector3(MenuRadius, MenuHeight, MenuDepth) * MenuScaleFactor * scale;
        }

        public static Vector3 MenuFoldedScale(float scale)
        {
            return new Vector3(MenuRadius, MenuHeight, 0f) * MenuScaleFactor * scale;
        }

        public static Vector3 NavScale => new Vector3(NavDepth, NavWidth, NavHeight);

        public static float ButtonZForSlot(int slot) => FirstButtonZ - slot * ButtonPitch;

        public void Draw()
        {
            if (WristMenu.Fonts == null)
            {
                WristMenu.InitFonts();
            }

            if (MenuRegistry.Instance.CurrentCategoryName == MenuRegistry.EnabledModsCategory)
            {
                WristMenu.RebuildEnabledMods();
            }

            WristMenu.Menu = new GameObject();
            WristMenu.Menu.transform.localScale =
                new Vector3(MenuRadius, MenuHeight, MenuDepth * MenuInitialDepthFactor);

            Text = new ModernTextLayer(WristMenu.Fonts);
            Text.Build(WristMenu.Menu.transform, LabelX);

            BuildBackdrop();
            BuildHeader();
            BuildDisconnect();
            BuildPage();
            BuildNav();
            BuildFooter();

            float scale = WristMenu.MenuCameraAnchored ? 1f : GameContext.PlayerScale;
            WristMenu.Menu.transform.localScale = MenuTargetScale(scale);
            WristMenu.SetLayerRecursive();
        }

        public void Teardown()
        {
            _roundedRenderers.Clear();
            _restScales.Clear();
        }

        public void UpdateVisual(string buttonId, string buttonText, bool isEnabled)
        {
            if (string.IsNullOrEmpty(buttonId) || WristMenu.Menu == null)
            {
                return;
            }

            Color baseColor = isEnabled ? WristMenu.ButtonColorEnabled : WristMenu.ButtonColorDisable;
            Color topColor = baseColor * 0.35f;

            foreach (Transform child in WristMenu.Menu.transform)
            {
                var button = child.GetComponent<MenuButton>();
                if (button == null || button.ButtonId != buttonId)
                {
                    continue;
                }

                Renderer renderer = child.GetComponent<Renderer>();
                if (renderer == (Object)null)
                {
                    break;
                }

                renderer.material = WristMenu.Materials.Gradient(topColor, baseColor);
                break;
            }

            if (_roundedRenderers.TryGetValue(buttonId, out List<Renderer> renderers) && renderers != null)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer != (Object)null)
                    {
                        renderer.material = WristMenu.Materials.Gradient(topColor, baseColor);
                    }
                }
            }

            Text?.SetColor(buttonId, isEnabled ? WristMenu.EnableTextColor : WristMenu.DisableTextColor);
        }

        public IReadOnlyList<Renderer> RoundedRenderersFor(string id)
        {
            return _roundedRenderers.TryGetValue(id, out List<Renderer> value) ? value : null;
        }

        public MenuAnimPlan BuildAnimPlan()
        {
            float scale = WristMenu.MenuCameraAnchored ? 1f : GameContext.PlayerScale;
            var plan = new MenuAnimPlan
            {
                TargetScale = MenuTargetScale(scale),
                FoldedScale = MenuFoldedScale(scale)
            };

            if (WristMenu.Menu == (Object)null)
            {
                return plan;
            }

            foreach (Transform child in WristMenu.Menu.transform)
            {
                var button = child.GetComponent<MenuButton>();
                if (button == null || string.IsNullOrEmpty(button.ButtonId))
                {
                    continue;
                }

                _restScales[button.ButtonId] = child.localScale;

                var item = new MenuAnimItem
                {
                    Body = child,
                    Visual = VisualFor(button.ButtonId),
                    Label = Text?.FindLabel(button.ButtonId),
                    BodyRest = child.localScale,
                    VisualRest = VisualRestFor(button.ButtonId),
                    LabelScale = t => Text.LabelScale(button.ButtonId, t)
                };

                if (button.ButtonId == ReservedButtonIds.Disconnect)
                {
                    plan.Disconnect = item;
                }
                else if (button.ButtonId != ReservedButtonIds.PreviousPage &&
                         button.ButtonId != ReservedButtonIds.NextPage)
                {
                    plan.Items.Add(item);
                }
            }

            return plan;
        }

        private Transform VisualFor(string id)
        {
            if (!_roundedRenderers.TryGetValue(id, out List<Renderer> renderers) ||
                renderers == null || renderers.Count == 0)
            {
                return null;
            }

            Renderer first = renderers[0];
            return first == (Object)null ? null : first.transform;
        }

        private Vector3 VisualRestFor(string id)
        {
            Transform visual = VisualFor(id);
            if (visual != null)
            {
                return visual.localScale;
            }

            return _restScales.TryGetValue(id, out Vector3 recorded) ? recorded : Vector3.one;
        }

        private void BuildBackdrop()
        {
            GameObject backdrop = Primitives.MakeCylinder();
            backdrop.transform.parent = WristMenu.Menu.transform;
            backdrop.transform.rotation = Quaternion.identity;
            backdrop.transform.localPosition = new Vector3(PanelX, 0f, 0f);
            backdrop.transform.localScale = new Vector3(PanelThickness, PanelWidth, PanelHeight);

            Color top = WristMenu.NormalColor * 0.35f;
            Color bottom = WristMenu.NormalColor;
            backdrop.GetComponent<Renderer>().material = WristMenu.Materials.Gradient(top, bottom);

            Round(backdrop, "__background__", top, bottom);
            WristMenu.MenuObj = backdrop;
        }

        private void BuildHeader()
        {
            Text.Add(ReservedButtonIds.Title, WristMenu.MenuTitle,
                new Vector3(LabelX, 0f, TitleZ), TitleWidth,
                TitleHeight, WristMenu.MenuTitleColor);
        }

        public void SetStatusVisible(bool visible)
        {
        }

        private void BuildDisconnect()
        {
            Color top = WristMenu.DisconnectButtonColor * 0.35f;
            Color bottom = WristMenu.DisconnectButtonColor;

            GameObject button = Primitives.MakeCylinderButton();
            button.transform.parent = WristMenu.Menu.transform;
            button.transform.rotation = Quaternion.identity;
            button.transform.localScale = new Vector3(ButtonDepth, ButtonWidth, ButtonHeight);
            button.transform.localPosition = new Vector3(PanelFrontX, 0f, DisconnectZ);
            var dcHitbox = button.GetComponent<BoxCollider>();
            if (dcHitbox != null)
            {
                dcHitbox.size = new Vector3(1f, 0.8f, 0.7f);
            }
            button.GetComponent<Renderer>().material = WristMenu.Materials.Gradient(top, bottom);

            var collider = button.AddComponent<MenuButton>();
            collider.ButtonId = ReservedButtonIds.Disconnect;
            collider.DisplayText = "Disconnect";

            Round(button, ReservedButtonIds.Disconnect, top, bottom);

            Text.AddStretched(ReservedButtonIds.Disconnect, "Disconnect",
                new Vector3(LabelX, 0f, DisconnectZ + LabelShiftZ),
                ButtonWidth * ButtonTextWidthRatio,
                ButtonHeight * ButtonTextHeightRatio,
                WristMenu.DisconnectTextColor);
        }

        private void BuildPage()
        {
            IReadOnlyList<ButtonInfo> buttons = MenuRegistry.Instance.CurrentButtons;
            if (buttons == null)
            {
                return;
            }

            int total = buttons.Count;
            int pageCount = PageSlicing.PageCount(total, PageSlicing.DefaultPageSize);
            WristMenu.PageNumber = PageSlicing.Clamp(WristMenu.PageNumber, pageCount);

            int first = PageSlicing.FirstIndex(WristMenu.PageNumber, PageSlicing.DefaultPageSize);
            int last = PageSlicing.LastIndex(first, total, PageSlicing.DefaultPageSize);

            for (int index = first; index < last; index++)
            {
                ButtonInfo button = buttons[index];
                if (button == null)
                {
                    continue;
                }

                int slot = index - first;
                string id = button.EffectiveId;
                bool isEnabled = button.enabled == true;
                Color baseColor = isEnabled ? WristMenu.ButtonColorEnabled : WristMenu.ButtonColorDisable;
                float z = ButtonZForSlot(slot);

                GameObject go = Primitives.MakeCylinderButton();
                go.transform.parent = WristMenu.Menu.transform;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = new Vector3(ButtonDepth, ButtonWidth, ButtonHeight);
                go.transform.localPosition = new Vector3(PanelFrontX, 0f, z);
                var hitbox = go.GetComponent<BoxCollider>();
                if (hitbox != null)
                {
                    hitbox.size = new Vector3(1f, 0.8f, 0.7f);
                }
                go.GetComponent<Renderer>().material = WristMenu.Materials.Gradient(baseColor * 0.35f, baseColor);

                var collider = go.AddComponent<MenuButton>();
                collider.ButtonId = id;
                collider.DisplayText = button.buttonText;

                Round(go, id, baseColor * 0.35f, baseColor);

                Color accent = isEnabled
                    ? new Color(WristMenu.ButtonColorEnabled.r, WristMenu.ButtonColorEnabled.g, WristMenu.ButtonColorEnabled.b, 0.75f)
                    : new Color(WristMenu.ButtonColorDisable.r, WristMenu.ButtonColorDisable.g, WristMenu.ButtonColorDisable.b, 0.4f);

                MakeOutline("frame_" + id, new Vector3(PanelFrontX + OutlineOffset, 0f, z),
                    new Vector3(OutlineDepth, ButtonWidth, ButtonHeight),
                    ButtonRadius, ButtonCornerSegments, OutlineBorder, accent);

                Text.Add(id, PageSlicing.Truncate(button.buttonText),
                    new Vector3(LabelX, LabelShiftY, z + LabelShiftZ),
                    LabelMaxWidth, LabelMaxHeight,
                    isEnabled ? WristMenu.EnableTextColor : WristMenu.DisableTextColor);
            }
        }

        private void BuildNav()
        {
            Color top = WristMenu.NextPrevButtonColor * 0.35f;
            Color bottom = WristMenu.NextPrevButtonColor;

            BuildNavButton(ReservedButtonIds.PreviousPage, "<", NavOffsetY, top, bottom);
            BuildNavButton(ReservedButtonIds.NextPage, ">", -NavOffsetY, top, bottom);
        }

        private void BuildNavButton(string id, string glyph, float y, Color top, Color bottom)
        {
            GameObject go = Primitives.MakeCylinderButton();
            go.transform.parent = WristMenu.Menu.transform;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = new Vector3(NavDepth, NavWidth, NavHeight);
            go.transform.localPosition = new Vector3(PanelFrontX, y, NavZ);
            var hitbox = go.GetComponent<BoxCollider>();
            if (hitbox != null)
            {
                hitbox.size = new Vector3(1f, 0.7f, 1f);
            }
            go.GetComponent<Renderer>().material = WristMenu.Materials.Gradient(top, bottom);

            var collider = go.AddComponent<MenuButton>();
            collider.ButtonId = id;
            collider.DisplayText = glyph;

            Round(go, id, top, bottom);

            MakeOutline("frame_" + id, new Vector3(PanelFrontX + OutlineOffset, y, NavZ),
                new Vector3(OutlineDepth, NavWidth, NavHeight),
                NavOutlineRadius, ButtonCornerSegments, OutlineBorder,
                new Color(
                    WristMenu.NextPrevButtonColor.r * 2.2f,
                    WristMenu.NextPrevButtonColor.g * 2.2f,
                    WristMenu.NextPrevButtonColor.b * 2.2f, 0.55f));

            Text.Add(id, glyph, new Vector3(LabelX, y, NavZ + LabelShiftZ),
                NavWidth * NavTextWidthRatio, NavHeight * NavTextHeightRatio, WristMenu.NextPrevTextColor);
        }

        private void BuildFooter()
        {
            WristMenu.FpsText = Text.Add(ReservedButtonIds.Status, WristMenu.BottomBarText,
                new Vector3(LabelX, 0f, StatusZ), StatusWidth, StatusHeight, WristMenu.ToolTipColor);
        }

        private void MakeOutline(string name, Vector3 localPosition, Vector3 localScale,
            float radius, int segments, float border, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(WristMenu.Menu.transform, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = localScale;

            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            filter.mesh = RoundedMeshFactory.RoundedFrame(radius, border, segments);
            renderer.material = WristMenu.Materials.Outline(color);
        }

        private void Round(GameObject go, string identifier, Color top, Color bottom)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == (Object)null)
            {
                return;
            }

            Vector3 localScale = go.transform.localScale;
            Vector3 localPosition = go.transform.localPosition;

            var rounded = new GameObject(identifier + "_rounded");
            rounded.transform.parent = WristMenu.Menu.transform;
            rounded.transform.rotation = Quaternion.identity;
            rounded.transform.localPosition = localPosition;
            rounded.transform.localScale = localScale;

            var filter = rounded.AddComponent<MeshFilter>();
            var meshRenderer = rounded.AddComponent<MeshRenderer>();
            filter.mesh = RoundedMeshFactory.RoundedRect(1f, 1f, BackgroundMeshRadius, BackgroundMeshSegments, BackgroundMeshDepth);
            meshRenderer.material = WristMenu.Materials.Gradient(top, bottom);

            _roundedRenderers[identifier] = new List<Renderer>(1) { meshRenderer };
            renderer.enabled = false;
        }
    }
}