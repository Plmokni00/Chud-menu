using GorillaLocomotion;
using Object = UnityEngine.Object;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using Chud.Diagnostics;
namespace Chud.Runtime
{
    public static class GameContext
    {
        public static GTPlayer Player => GTPlayer.Instance;

        public static bool HasPlayer => GTPlayer.Instance != (Object)null;

        public static GorillaTagger Tagger => GorillaTagger.Instance;

        public static bool HasTagger
        {
            get
            {
                GorillaTagger tagger = GorillaTagger.Instance;
                return tagger != (Object)null && tagger.headCollider != (Object)null;
            }
        }

        public static VRRig LocalRig => VRRig.LocalRig;

        public static bool HasLocalRig => VRRig.LocalRig != (Object)null;

        public static bool TryGetHeadTransform(out VRRig rig, out Transform head)
        {
            rig = VRRig.LocalRig;
            if (rig != (Object)null && rig.head != null && rig.head.rigTarget != (Object)null)
            {
                head = rig.head.rigTarget.transform;
                return true;
            }

            head = null;
            return false;
        }

        public static NetworkSystem Network => NetworkSystem.Instance;

        public static bool HasNetwork => NetworkSystem.Instance != null;

        public static NetPlayer LocalNetPlayer
        {
            get
            {
                NetworkSystem net = NetworkSystem.Instance;
                return net == null ? null : net.LocalPlayer;
            }
        }

        public static Player LocalPhotonPlayer
        {
            get
            {
                return PhotonNetwork.LocalPlayer;
            }
        }

        public static bool InRoom
        {
            get
            {
                return Log.Guard("GameContext.InRoom", () => PhotonNetwork.InRoom, false);
            }
        }

        public static GorillaNetworking.PhotonNetworkController JoinController =>
            GorillaNetworking.PhotonNetworkController.Instance;

        public static ControllerInputPoller Poller => ControllerInputPoller.instance;

        public static bool HasPoller => ControllerInputPoller.instance != (Object)null;

        public static GorillaGameManager GameMode => GorillaGameManager.instance;

        public static GorillaNetworking.CosmeticsController Cosmetics => GorillaNetworking.CosmeticsController.instance;

        public static GorillaTagManager Infection
        {
            get
            {
                GorillaGameManager manager = GorillaGameManager.instance;
                return manager as GorillaTagManager;
            }
        }

        public static GorillaGuardianManager Guardian
        {
            get
            {
                GorillaGameManager manager = GorillaGameManager.instance;
                return manager as GorillaGuardianManager;
            }
        }

        public static GorillaPaintbrawlManager Paintbrawl
        {
            get
            {
                GorillaGameManager manager = GorillaGameManager.instance;
                return manager as GorillaPaintbrawlManager;
            }
        }

        public static bool IsVrActive
        {
            get
            {
                try
                {
                    return UnityEngine.XR.XRSettings.isDeviceActive;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public static bool IsFlatscreen => !IsVrActive;

        public static float PlayerScale
        {
            get
            {
                GTPlayer player = GTPlayer.Instance;
                if (player == (Object)null)
                {
                    return 1f;
                }

                float scale = player.scale;
                return SanitizeScale(scale);
            }
        }

        public static float MenuScale(bool cameraAnchored)
        {
            return cameraAnchored ? 1f : PlayerScale;
        }

        public static float SanitizeScale(float scale, float fallback = 1f)
        {
            if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
            {
                return fallback;
            }

            return scale;
        }
    }
}