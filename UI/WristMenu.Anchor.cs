using Chud.Backend;
using Chud.Diagnostics;
using Chud.Runtime;
using GorillaLocomotion;
using Object = UnityEngine.Object;
using UnityEngine;
namespace Chud.UI
{
    internal partial class WristMenu
    {
        private static readonly Vector3 CameraDownOffset = Vector3.down * 0.03f;
        private static readonly Quaternion CameraMenuRotation = Quaternion.Euler(-90f, 90f, 0f);
        private static readonly Quaternion RightHandMenuRotation = Quaternion.Euler(0f, 0f, 180f);
        private const float AnchorUpHeight = 0.02f;

        private static Vector3 AnchorUpOffset => Vector3.up * (AnchorUpHeight * GameContext.PlayerScale);
        private const float CameraForwardDistance = 0.5f;

        public static void ReanchorToCurrentHand()
        {
            if (MenuCameraAnchored)
            {
                return;
            }

            if (Menu == (Object)null || Close)
            {
                return;
            }

            bool rightHanded = Mods.IsRightHanded;
            Transform hand = RightHandedController(rightHanded);
            if (hand == null)
            {
                return;
            }

            SetMenuAnchor(MenuAnchor != (Object)null ? MenuAnchor : new GameObject("menuAnchor"), rightHanded, hand);
            RefreshMenu();
        }

        internal static Transform RightHandedController(bool rightHanded)
        {
            GTPlayer player = GameContext.Player;
            if (player == (Object)null)
            {
                return null;
            }

            return rightHanded ? player.RightHand.controllerTransform : player.LeftHand.controllerTransform;
        }

        internal static Transform LeftHandedController(bool rightHanded)
        {
            GTPlayer player = GameContext.Player;
            if (player == (Object)null)
            {
                return null;
            }

            return rightHanded ? player.LeftHand.controllerTransform : player.RightHand.controllerTransform;
        }

        internal static void RestoreMenuAnchor()
        {
            if (Menu == (Object)null)
            {
                return;
            }

            if (MenuCameraAnchored)
            {
                AttachToCamera();
            }
            else if (MenuAnchor != (Object)null)
            {
                AttachToHand();
            }
        }

        private static void AttachToCamera()
        {
            Transform anchor = ResolveCameraAnchor();
            if (anchor == (Object)null)
            {
                return;
            }

            Menu.transform.parent = anchor;
            Menu.transform.position = anchor.position + anchor.forward * CameraForwardDistance + CameraDownOffset;
            Menu.transform.rotation = anchor.rotation * CameraMenuRotation;

            AttachPointer(RightHandedController(true));
        }

        private static void AttachToHand()
        {
            Transform follow = MenuFollowHand ?? RightHandedController(MenuAnchorIsRightHand);
            if (follow == (Object)null)
            {
                return;
            }

            Menu.transform.parent = MenuAnchor.transform;
            Menu.transform.localPosition = Vector3.zero;
            Menu.transform.localRotation = MenuAnchorIsRightHand ? RightHandMenuRotation : Quaternion.identity;
            MenuAnchor.transform.position = follow.position + AnchorUpOffset;
            MenuAnchor.transform.rotation = follow.rotation;

            AttachPointer(LeftHandedController(MenuAnchorIsRightHand));
        }

        private static void AttachPointer(Transform hand)
        {
            EnsureReference();
            if (hand == (Object)null || Reference == (Object)null)
            {
                return;
            }

            Reference.transform.parent = hand;
            Reference.transform.localPosition = PointerPos;
            Reference.transform.localScale = PointerScale;
        }

        private static Transform ResolveCameraAnchor()
        {
            Camera tpc = ThirdPersonCamera;
            if (tpc != null)
            {
                return tpc.transform;
            }

            GorillaTagger tagger = GameContext.Tagger;
            if (tagger != (Object)null && tagger.headCollider != (Object)null)
            {
                return tagger.headCollider.transform;
            }

            return null;
        }

        internal static void EnsureReference()
        {
            if (Reference == (Object)null)
            {
                Reference = Primitives.MakeSphereButtonPresser();
                Reference.name = "buttonPresser";
            }
        }

        internal static void EnsureAnchor(bool rightHanded, Transform follow)
        {
            if (MenuAnchor == (Object)null)
            {
                SetMenuAnchor(new GameObject("menuAnchor"), rightHanded, follow);
                return;
            }

            if (MenuAnchorIsRightHand != rightHanded || MenuFollowHand != follow)
            {
                SetMenuAnchor(MenuAnchor, rightHanded, follow);
            }
        }

        internal static void AttachToNewHand(bool rightHanded)
        {
            if (Menu == (Object)null)
            {
                return;
            }

            Transform follow = RightHandedController(rightHanded);
            if (follow == (Object)null)
            {
                return;
            }

            EnsureAnchor(rightHanded, follow);

            Menu.transform.parent = MenuAnchor.transform;
            Menu.transform.localPosition = Vector3.zero;
            Menu.transform.localRotation = rightHanded ? RightHandMenuRotation : Quaternion.identity;
            MenuAnchor.transform.position = follow.position + AnchorUpOffset;
            MenuAnchor.transform.rotation = follow.rotation;

            AttachPointer(LeftHandedController(rightHanded));
        }

        internal static void FollowAnchor()
        {
            if (MenuAnchor == (Object)null || MenuFollowHand == (Object)null)
            {
                return;
            }

            MenuAnchor.transform.position = MenuFollowHand.position + AnchorUpOffset;
            MenuAnchor.transform.rotation = MenuFollowHand.rotation;
            ApplyLiveMenuScale();
        }

        internal static void ApplyLiveMenuScale()
        {
            if (Menu == (Object)null || MenuCameraAnchored || AnimatorOwnsScale || Close)
            {
                return;
            }

            Menu.transform.localScale = IsNormal
                ? NormalLayout.MenuTargetScale(GameContext.PlayerScale)
                : ModernLayout.MenuTargetScale(GameContext.PlayerScale);
        }
    }
}