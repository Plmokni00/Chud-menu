using Chud.Menu;
using Chud.Rendering;
using Chud.Runtime;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
namespace Chud.UI
{
    internal sealed class NormalLayout
    {
        public const string Id = "normal";

        public const float MenuRadius = 0.1f;
        public const float MenuHeight = 0.3f;
        public const float MenuDepth = 0.4f;
        public const float MenuInitialDepthFactor = 0.95625f;
        public const float MenuScaleFactor = 0.8f;

        public const float ButtonScaleX = 0.09f;
        public const float ButtonScaleY = 0.9f;
        public const float ButtonScaleZ = 0.08f;

        public const float NavScaleX = 0.09f;
        public const float NavScaleY = 0.2f;
        public const float NavScaleZ = 0.9f;

        public const float PanelPosX = 0.05f;
        public const float SurfaceX = 0.56f;
        public const float DisconnectZ = 0.6f;
        public const float NavY = 0.65f;
        public const float FirstButtonZ = 0.28f;

        public const float TitlePosX = 0.06f;
        public const float TitlePosZ = 0.175f;
        public const float TitleWidth = 0.28f;
        public const float TitleHeight = 0.05f;

        public const float StatusPosX = 0.06f;
        public const float StatusPosZ = 0.135f;
        public const float StatusWidth = 0.28f;
        public const float StatusHeight = 0.02f;

        public const float LabelPosX = 0.064f;
        public const float LabelWidth = 0.2f;
        public const float LabelHeight = 0.03f;
        public const float LabelBaseZ = 0.111f;
        public const float LabelCompress = 2.6f;
        public const float NavLabelY = 0.195f;

        public const float SlotPitch = 0.116f;

        private const float ButtonMeshRadius = 0.08f;
        private const int ButtonMeshSegments = 6;
        private const float ButtonMeshDepth = 0.85f;

        private readonly Dictionary<string, List<Renderer>> _roundedRenderers =
            new Dictionary<string, List<Renderer>>(32);

        public NormalTextLayer Text { get; private set; }

        public static Vector3 MenuTargetScale(float scale)
        {
            return new Vector3(MenuRadius, MenuHeight, MenuDepth) * MenuScaleFactor * scale;
        }

        public static Vector3 MenuFoldedScale(float scale)
        {
            return new Vector3(MenuRadius, MenuHeight, 0f) * MenuScaleFactor * scale;
        }

        public static Vector3 ButtonScale()
        {
            return new Vector3(ButtonScaleX, ButtonScaleY, ButtonScaleZ);
        }

        public static Vector3 NavScale()
        {
            return new Vector3(NavScaleX, NavScaleY, NavScaleZ);
        }

        public void Draw()
        {
            if (WristMenu.Fonts == null)
            {
                WristMenu.InitFonts();
            }

            Text = new NormalTextLayer();

            if (MenuRegistry.Instance.CurrentCategoryName == MenuRegistry.EnabledModsCategory)
            {
                WristMenu.RebuildEnabledMods();
            }

            WristMenu.Menu = new GameObject();
            WristMenu.Menu.transform.localScale =
                new Vector3(MenuRadius, MenuHeight, MenuDepth * MenuInitialDepthFactor);

            BuildBackdrop();
            Text.Build(WristMenu.Menu.transform);
            BuildHeader();
            BuildDisconnect();
            BuildNav();
            BuildPage();
            ApplyFinalScale();
        }

        public void Teardown()
        {
            _roundedRenderers.Clear();
        }

        public void UpdateVisual(string buttonId, bool isEnabled)
        {
            if (string.IsNullOrEmpty(buttonId) || WristMenu.Menu == null)
            {
                return;
            }

            Color baseColor = isEnabled ? WristMenu.ButtonColorEnabled : WristMenu.ButtonColorDisable;
            Color topColor = baseColor * 0.35f;

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
            var plan = new MenuAnimPlan
            {
                TargetScale = MenuTargetScale(GameContext.PlayerScale),
                FoldedScale = MenuFoldedScale(GameContext.PlayerScale)
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

                var item = new MenuAnimItem
                {
                    Body = child,
                    Visual = VisualFor(button.ButtonId),
                    Label = Text?.FindLabel(button.ButtonId),
                    BodyRest = IsNav(button.ButtonId) ? NavScale() : ButtonScale(),
                    VisualRest = IsNav(button.ButtonId) ? NavScale() : ButtonScale(),
                    LabelScale = t => new Vector3(t, t, t)
                };

                if (button.ButtonId == ReservedButtonIds.Disconnect)
                {
                    plan.Disconnect = item;
                }
                else if (!IsNav(button.ButtonId))
                {
                    plan.Items.Add(item);
                }
            }

            return plan;
        }

        private static bool IsNav(string id)
        {
            return id == ReservedButtonIds.PreviousPage || id == ReservedButtonIds.NextPage;
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

        public Mesh ButtonMesh => RoundedMeshFactory.RoundedRect(1f, 1f, ButtonMeshRadius, ButtonMeshSegments, ButtonMeshDepth);

        private void BuildBackdrop()
        {
            GameObject backdrop = Primitives.MakeCylinder();
            backdrop.transform.parent = WristMenu.Menu.transform;
            backdrop.transform.rotation = Quaternion.identity;
            backdrop.transform.localScale = new Vector3(0.1f, 1f, 1f);

            Color top = WristMenu.NormalColor * 0.35f;
            Color bottom = WristMenu.NormalColor;
            backdrop.GetComponent<Renderer>().material = WristMenu.Materials.Gradient(top, bottom);
            backdrop.transform.position = new Vector3(PanelPosX, 0f, 0f);

            Round(backdrop, "__background__", top, bottom);
            WristMenu.MenuObj = backdrop;
        }

        private void BuildHeader()
        {
            var titleObj = new GameObject();
            titleObj.transform.parent = WristMenu.CanvasObj.transform;
            Text title = titleObj.AddComponent<Text>();
            title.font = WristMenu.Fonts.Get(NormalTextLayer.FontSize);
            title.text = WristMenu.MenuTitle;
            title.fontSize = NormalTextLayer.FontSize;
            title.color = WristMenu.MenuTitleColor;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = NormalTextLayer.FontMinSize;
            title.resizeTextMaxSize = NormalTextLayer.FontMaxSize;

            var titleRect = title.GetComponent<RectTransform>();
            titleRect.localPosition = Vector3.zero;
            titleRect.sizeDelta = new Vector2(TitleWidth, TitleHeight);
            titleRect.position = new Vector3(TitlePosX, 0f, TitlePosZ);
            titleRect.rotation = NormalTextLayer.Rotation;
            Text.Track(ReservedButtonIds.Title, title);

            var statusObj = new GameObject();
            statusObj.transform.parent = WristMenu.CanvasObj.transform;
            Text status = statusObj.AddComponent<Text>();
            status.font = WristMenu.Fonts.Get(NormalTextLayer.FontSize);
            status.text = WristMenu.BottomBarText;
            status.fontSize = NormalTextLayer.FontSize;
            status.color = WristMenu.ToolTipColor;
            status.fontStyle = FontStyle.Bold;
            status.alignment = TextAnchor.MiddleCenter;
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = NormalTextLayer.FontMinSize;
            status.resizeTextMaxSize = NormalTextLayer.FontMaxSize;

            var statusRect = status.GetComponent<RectTransform>();
            statusRect.localPosition = Vector3.zero;
            statusRect.sizeDelta = new Vector2(StatusWidth, StatusHeight);
            statusRect.position = new Vector3(StatusPosX, 0f, StatusPosZ);
            statusRect.rotation = NormalTextLayer.Rotation;

            WristMenu.FpsText = status;
            Text.Track(ReservedButtonIds.Status, status);
        }

        public void SetStatusVisible(bool visible)
        {
        }

        private void BuildDisconnect()
        {
            Color top = WristMenu.DisconnectButtonColor * 0.35f;
            Color bottom = WristMenu.DisconnectButtonColor;

            AddButton(ReservedButtonIds.Disconnect, "Disconnect", ButtonScale(),
                new Vector3(SurfaceX, 0f, DisconnectZ), top, bottom);

            float labelZ = LabelBaseZ - (FirstButtonZ - DisconnectZ) / LabelCompress;
            Text.Add(ReservedButtonIds.Disconnect, "Disconnect",
                new Vector2(LabelWidth, LabelHeight), new Vector3(LabelPosX, 0f, labelZ),
                WristMenu.DisconnectTextColor, true);
        }

        private void BuildNav()
        {
            Color top = WristMenu.NextPrevButtonColor * 0.35f;
            Color bottom = WristMenu.NextPrevButtonColor;

            AddButton(ReservedButtonIds.PreviousPage, "<", NavScale(),
                new Vector3(SurfaceX, NavY, 0f), top, bottom);
            Text.Add(ReservedButtonIds.PreviousPage, "<",
                new Vector2(LabelWidth, LabelHeight), new Vector3(LabelPosX, NavLabelY, 0f),
                WristMenu.NextPrevTextColor, false);

            AddButton(ReservedButtonIds.NextPage, ">", NavScale(),
                new Vector3(SurfaceX, -NavY, 0f), top, bottom);
            Text.Add(ReservedButtonIds.NextPage, ">",
                new Vector2(LabelWidth, LabelHeight), new Vector3(LabelPosX, -NavLabelY, 0f),
                WristMenu.NextPrevTextColor, false);
        }

        private void BuildPage()
        {
            IReadOnlyList<ButtonInfo> buttons = MenuRegistry.Instance.CurrentButtons;
            if (buttons == null || buttons.Count == 0)
            {
                return;
            }

            int pageSize = PageSlicing.DefaultPageSize;
            int total = buttons.Count;
            int pageCount = PageSlicing.PageCount(total, pageSize);
            WristMenu.PageNumber = PageSlicing.Clamp(WristMenu.PageNumber, pageCount);

            int first = PageSlicing.FirstIndex(WristMenu.PageNumber, pageSize);
            int last = PageSlicing.LastIndex(first, total, pageSize);

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

                float slotOffset = slot * SlotPitch;
                float z = FirstButtonZ - slotOffset;

                AddButton(id, button.buttonText, ButtonScale(),
                    new Vector3(SurfaceX, 0f, z), baseColor * 0.35f, baseColor);

                Text.Add(id, button.buttonText,
                    new Vector2(LabelWidth, LabelHeight),
                    new Vector3(LabelPosX, 0f, LabelBaseZ - slotOffset / LabelCompress),
                    isEnabled ? WristMenu.EnableTextColor : WristMenu.DisableTextColor,
                    true);
            }
        }

        private void AddButton(string id, string display, Vector3 scale, Vector3 pos, Color top, Color bottom)
        {
            GameObject go = Primitives.MakeConvexRoundedButton(ButtonMesh);
            go.transform.parent = WristMenu.Menu.transform;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = scale;
            go.transform.localPosition = pos;
            go.GetComponent<Renderer>().material = WristMenu.Materials.Gradient(top, bottom);

            var collider = go.AddComponent<MenuButton>();
            collider.ButtonId = id;
            collider.DisplayText = display;

            Round(go, id, top, bottom);
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
            filter.mesh = ButtonMesh;
            meshRenderer.material = WristMenu.Materials.Gradient(top, bottom);

            _roundedRenderers[identifier] = new List<Renderer>(1) { meshRenderer };
            renderer.enabled = false;
        }

        private static void ApplyFinalScale()
        {
            WristMenu.Menu.transform.localScale = MenuTargetScale(GameContext.PlayerScale);
            WristMenu.SetLayerRecursive();
        }
    }
}