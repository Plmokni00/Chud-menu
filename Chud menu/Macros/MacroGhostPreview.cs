using System.Collections.Generic;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Chud.Backend
{
    public sealed class MacroGhostPreview
    {
        private static Material _sharedGhostMaterial;

        private readonly VRRig _rig;
        private GameObject _nametag;
        private Vector3 _headPos;

        private ChudRigTransform _old;
        private ChudRigTransform _target;

        public float LastUpdateDelay = 0.1f;
        public float LastUpdateTime;

        public MacroGhostPreview(ChudRigTransform start, string name, Color tint)
        {
            VRRig local = VRRig.LocalRig;
            VRRig clone = null;

            if (local != null)
            {
                GameObject holder = new GameObject("Chud_GhostRigHolder");
                holder.SetActive(false);
                try { Mods.cloningGhostRig = true; } catch { }
                try
                {
                    clone = (VRRig)Object.Instantiate(local,
                        start.RigPosition, start.RigRotation, holder.transform);
                }
                finally
                {
                    try { Mods.cloningGhostRig = false; } catch { }
                }

                if (clone != null)
                {
                    clone.isOfflineVRRig = true;
                    clone.gameObject.name = "Chud_GhostRig";
                    clone.gameObject.SetActive(false);
                    try
                    {
                        Transform localParent = ((Component)local).transform.parent;
                        clone.transform.SetParent(localParent);
                    }
                    catch { }
                    Object.Destroy(holder);

                    StripClone(clone);
                    ApplyGhostMaterial(clone);
                    clone.gameObject.SetActive(true);
                }
                else
                {
                    Object.Destroy(holder);
                }
            }

            _rig = clone;

            if (_rig != null)
            {
                SnapTo(start);
                _headPos = start.HeadPosition;
                _nametag = CreateNametag(name ?? "macro", tint);
                TickTint();
            }

            LastUpdateTime = Time.time;
        }

        public void SetVisible(bool visible)
        {
            try
            {
                if (_rig != null && _rig.gameObject != null)
                    _rig.gameObject.SetActive(visible);
            }
            catch { }
            try
            {
                if (_nametag != null)
                    _nametag.SetActive(visible);
            }
            catch { }
        }

        public void SnapTo(ChudRigTransform t)
        {
            _old = _target = t;
            ApplyTransform(1f, _old, t);
        }

        public void UpdateTargets(Vector3 headPos, Quaternion headRot, Vector3 leftPos, Quaternion leftRot,
            Vector3 rightPos, Quaternion rightRot)
        {
            LastUpdateDelay = Mathf.Max(Time.time - LastUpdateTime, 0.01f);
            LastUpdateTime = Time.time;

            _old = _target;
            _target = new ChudRigTransform(headPos, headRot, _target.RigPosition, _target.RigRotation,
                leftPos, leftRot, rightPos, rightRot, Vector3.zero);
        }

        public void Tick()
        {
            if (_rig == null)
                return;

            float t = (Time.time - LastUpdateTime) / LastUpdateDelay;
            t = Mathf.Clamp01(t);
            ApplyTransform(t, _old, _target);
            TickTint();

            if (_nametag != null)
            {
                try
                {
                    _nametag.transform.position = _headPos + Vector3.up * 0.3f;
                    if (Camera.main != null)
                    {
                        _nametag.transform.LookAt(Camera.main.transform.position);
                        _nametag.transform.Rotate(0f, 180f, 0f);
                    }
                }
                catch { }
            }
        }

        public void Destroy()
        {
            try
            {
                if (_rig != null && _rig.gameObject != null)
                    Object.Destroy(_rig.gameObject);
            }
            catch { }
            try
            {
                if (_nametag != null)
                    Object.Destroy(_nametag);
            }
            catch { }
        }

        private void ApplyTransform(float t, ChudRigTransform from, ChudRigTransform to)
        {
            if (_rig == null)
                return;

            try
            {
                _rig.transform.position = Vector3.Lerp(from.RigPosition, to.RigPosition, t);
                _rig.transform.rotation = Quaternion.Lerp(from.RigRotation, to.RigRotation, t);

                Vector3 headPos = Vector3.Lerp(from.HeadPosition, to.HeadPosition, t);
                Quaternion headRot = Quaternion.Lerp(from.HeadRotation, to.HeadRotation, t);
                if (_rig.head != null && _rig.head.rigTarget != null)
                    _rig.head.rigTarget.SetPositionAndRotation(headPos, headRot);
                _headPos = headPos;

                if (_rig.leftHand != null && _rig.leftHand.rigTarget != null)
                {
                    _rig.leftHand.rigTarget.position =
                        Vector3.Lerp(from.LeftHandPosition, to.LeftHandPosition, t);
                    _rig.leftHand.rigTarget.rotation =
                        Quaternion.Lerp(from.LeftHandRotation, to.LeftHandRotation, t);
                }
                if (_rig.rightHand != null && _rig.rightHand.rigTarget != null)
                {
                    _rig.rightHand.rigTarget.position =
                        Vector3.Lerp(from.RightHandPosition, to.RightHandPosition, t);
                    _rig.rightHand.rigTarget.rotation =
                        Quaternion.Lerp(from.RightHandRotation, to.RightHandRotation, t);
                }
            }
            catch { }
        }

        private void TickTint()
        {
            try
            {
                if (_rig == null || _sharedGhostMaterial == null)
                    return;
                Color want = Color.white;
                try
                {
                    if (VRRig.LocalRig != null)
                        want = VRRig.LocalRig.playerColor;
                }
                catch { }
                if (want.r < 4f / 255f && want.g < 4f / 255f && want.b < 4f / 255f)
                    want = Color.white;
                want.a = 0.5f;
                _sharedGhostMaterial.color = want;
            }
            catch { }
        }

        private static void StripClone(VRRig ghost)
        {
            try
            {
                Transform slideL = ghost.transform.Find("VR Constraints/LeftArm/Left Arm IK/SlideAudio");
                if (slideL != null) slideL.gameObject.SetActive(false);
                Transform slideR = ghost.transform.Find("VR Constraints/RightArm/Right Arm IK/SlideAudio");
                if (slideR != null) slideR.gameObject.SetActive(false);
                Transform slideB = ghost.transform.Find("rig/body_pivot/SlideAudio");
                if (slideB != null) slideB.gameObject.SetActive(false);
            }
            catch { }

            try
            {
                VRRig[] childRigs = ghost.GetComponentsInChildren<VRRig>(true);
                foreach (VRRig child in childRigs)
                    if (child != ghost)
                        Object.Destroy(child.gameObject);
            }
            catch { }
            try
            {
                foreach (AutoSyncTransforms sync in ghost.GetComponentsInChildren<AutoSyncTransforms>(true))
                {
                    sync.enabled = false;
                    Object.Destroy(sync);
                }
            }
            catch { }
            try
            {
                foreach (PhotonView pv in ghost.GetComponentsInChildren<PhotonView>(true))
                    Object.Destroy(pv);
            }
            catch { }
            try
            {
                foreach (PhotonTransformView ptv in ghost.GetComponentsInChildren<PhotonTransformView>(true))
                    Object.Destroy(ptv);
            }
            catch { }
            try
            {
                foreach (Component c in ghost.GetComponentsInChildren<Component>(true))
                {
                    if (c == null || c is Transform) continue;
                    string tn = c.GetType().Name;
                    if (tn == "TagEffectsPackToggle" || tn == "RigidbodyWaterInteraction" || tn == "ConstantForce")
                        Object.Destroy(c);
                }
            }
            catch { }
            try
            {
                foreach (Collider col in ghost.GetComponentsInChildren<Collider>(true))
                    Object.Destroy(col);
            }
            catch { }
            try
            {
                foreach (Rigidbody rb in ghost.GetComponentsInChildren<Rigidbody>(true))
                    Object.Destroy(rb);
            }
            catch { }
            try
            {
                foreach (GorillaPlayerScoreboardLine line in ghost.GetComponentsInChildren<GorillaPlayerScoreboardLine>(true))
                    Object.Destroy(line);
            }
            catch { }

            try
            {
                CosmeticsController.CosmeticSet emptySet = new CosmeticsController.CosmeticSet();
                if (CosmeticsController.instance != null)
                    emptySet.ClearSet(CosmeticsController.instance.nullItem);
                ghost.cosmeticSet = emptySet;
                ghost.cosmeticsObjectRegistry = new CosmeticItemRegistry(ghost);

                List<GameObject> cosmeticObjects = new List<GameObject>();
                foreach (PlayerColoredCosmetic cosmetic in ghost.GetComponentsInChildren<PlayerColoredCosmetic>(true))
                    cosmeticObjects.Add(cosmetic.gameObject);
                foreach (HoldableObject holdable in ghost.GetComponentsInChildren<HoldableObject>(true))
                    cosmeticObjects.Add(holdable.gameObject);
                foreach (GameObject cosmeticObject in cosmeticObjects)
                {
                    if (cosmeticObject != null)
                    {
                        cosmeticObject.SetActive(false);
                        Object.Destroy(cosmeticObject);
                    }
                }
            }
            catch { }
        }

        private static void ApplyGhostMaterial(VRRig ghost)
        {
            try
            {
                if (_sharedGhostMaterial == null)
                {
                    Shader s = Shader.Find("GorillaTag/UberShader");
                    if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
                    if (s == null) s = Shader.Find("GUI/Text Shader");
                    _sharedGhostMaterial = new Material(s);
                    _sharedGhostMaterial.hideFlags = HideFlags.HideAndDontSave;
                    if (_sharedGhostMaterial.HasProperty("_Surface"))
                        _sharedGhostMaterial.SetFloat("_Surface", 1f);
                    if (_sharedGhostMaterial.HasProperty("_Blend"))
                        _sharedGhostMaterial.SetFloat("_Blend", 0f);
                    if (_sharedGhostMaterial.HasProperty("_SrcBlend"))
                        _sharedGhostMaterial.SetFloat("_SrcBlend", 5f);
                    if (_sharedGhostMaterial.HasProperty("_DstBlend"))
                        _sharedGhostMaterial.SetFloat("_DstBlend", 10f);
                    if (_sharedGhostMaterial.HasProperty("_ZWrite"))
                        _sharedGhostMaterial.SetFloat("_ZWrite", 0f);
                    _sharedGhostMaterial.renderQueue = 3000;
                }

                if (ghost.mainSkin != null)
                    ghost.mainSkin.material = _sharedGhostMaterial;

                Renderer[] renderers = ghost.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers)
                {
                    if (r == null || r is SkinnedMeshRenderer == false)
                        continue;
                    try { r.material = _sharedGhostMaterial; } catch { }
                }

                try
                {
                    Color want = Color.white;
                    if (VRRig.LocalRig != null)
                        want = VRRig.LocalRig.playerColor;
                    if (want.r < 4f / 255f && want.g < 4f / 255f && want.b < 4f / 255f)
                        want = Color.white;
                    want.a = 0.5f;
                    _sharedGhostMaterial.color = want;
                }
                catch { }
            }
            catch { }
        }

        private static GameObject CreateNametag(string name, Color colour)
        {
            GameObject tag = new GameObject("ChudMacro_GhostNametag");
            try
            {
                Canvas canvas = tag.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.localScale = Vector3.one * 0.003f;

                Text text = tag.AddComponent<Text>();
                try
                {
                    if (Mods.comicSansFont != null)
                        text.font = Mods.comicSansFont;
                }
                catch { }
                text.fontSize = 30;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.alignment = TextAnchor.MiddleCenter;
                text.text = name ?? "macro";
                text.color = colour;
            }
            catch { }
            return tag;
        }
    }
}
