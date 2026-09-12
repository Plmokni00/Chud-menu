using UnityEngine;
using UnityEngine.UI;

namespace Chud.Backend
{
    public sealed class MacroFakeRig
    {
        private readonly GameObject _nametag;
        private readonly GameObjectData[] _parts;
        private readonly bool _showNametag;

        public float LastUpdateDelay = 0.1f;
        public float LastUpdateTime;

        public MacroFakeRig(Color colour, Vector3 headPos, Quaternion headRot,
            Vector3 leftPos, Quaternion leftRot, Vector3 rightPos, Quaternion rightRot,
            string name, bool showNametag)
        {
            GameObject head = CreateHead(colour);
            GameObject leftHand = CreateSphere(0.1f, colour);
            GameObject rightHand = CreateSphere(0.1f, colour);

            if (showNametag)
                _nametag = CreateNametag(name, colour, head.transform);

            _showNametag = showNametag;

            _parts = new GameObjectData[]
            {
                new GameObjectData(head, headPos, headRot),
                new GameObjectData(leftHand, leftPos, leftRot),
                new GameObjectData(rightHand, rightPos, rightRot),
            };

            LastUpdateTime = Time.time;
        }

        public void UpdateTargets(Vector3 headPos, Quaternion headRot, Vector3 leftPos, Quaternion leftRot,
            Vector3 rightPos, Quaternion rightRot)
        {
            LastUpdateDelay = Mathf.Max(Time.time - LastUpdateTime, 0.01f);
            LastUpdateTime = Time.time;

            _parts[0].SetTarget(headPos, headRot * Quaternion.Euler(90f, 0f, 0f));
            _parts[1].SetTarget(leftPos, leftRot);
            _parts[2].SetTarget(rightPos, rightRot);
        }

        public void Tick()
        {
            float t = (Time.time - LastUpdateTime) / LastUpdateDelay;
            foreach (GameObjectData part in _parts)
                part.Interpolate(t);

            if (Camera.main == null || !_showNametag || _nametag == null)
                return;

            _nametag.transform.LookAt(Camera.main.transform.position);
            _nametag.transform.Rotate(0f, 180f, 0f);
        }

        public void Destroy()
        {
            foreach (GameObjectData part in _parts)
            {
                if (part.AssociatedGameObject != null)
                    Object.Destroy(part.AssociatedGameObject);
            }
            if (_nametag != null)
                Object.Destroy(_nametag);
        }

        private static GameObject CreateSphere(float scale, Color colour)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(obj.GetComponent<Collider>());
            obj.transform.localScale = Vector3.one * scale;
            Renderer r = obj.GetComponent<Renderer>();
            if (r != null)
                r.material.color = colour;
            return obj;
        }

        private static GameObject CreateHead(Color colour)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Object.Destroy(obj.GetComponent<Collider>());
            obj.transform.localScale = Vector3.one * 0.2f;
            Renderer r = obj.GetComponent<Renderer>();
            if (r != null)
                r.material.color = colour;
            return obj;
        }

        private static GameObject CreateNametag(string name, Color colour, Transform parent)
        {
            GameObject tag = new GameObject("ChudMacro_Nametag");
            tag.transform.SetParent(parent);
            tag.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            tag.transform.localScale = Vector3.one;

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

            return tag;
        }

        private struct GameObjectData
        {
            public readonly GameObject AssociatedGameObject;

            private Vector3 _oldPos;
            private Vector3 _targetPos;
            private Quaternion _oldRot;
            private Quaternion _targetRot;

            public GameObjectData(GameObject obj, Vector3 pos, Quaternion rot)
            {
                AssociatedGameObject = obj;
                _oldPos = _targetPos = pos;
                _oldRot = _targetRot = rot;
            }

            public void SetTarget(Vector3 pos, Quaternion rot)
            {
                _oldPos = _targetPos;
                _oldRot = _targetRot;
                _targetPos = pos;
                _targetRot = rot;
            }

            public void Interpolate(float t)
            {
                if (AssociatedGameObject == null)
                    return;
                AssociatedGameObject.transform.position = Vector3.Lerp(_oldPos, _targetPos, t);
                AssociatedGameObject.transform.rotation = Quaternion.Lerp(_oldRot, _targetRot, t);
            }
        }
    }
}
