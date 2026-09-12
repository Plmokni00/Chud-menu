using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chud.Backend
{
    public static class MacroJsonHelpers
    {
        public static JObject FromVector3(Vector3 v)
        {
            return new JObject
            {
                ["x"] = v.x,
                ["y"] = v.y,
                ["z"] = v.z,
            };
        }

        public static Vector3 ToVector3(JObject o)
        {
            return new Vector3(
                o["x"].ToObject<float>(),
                o["y"].ToObject<float>(),
                o["z"].ToObject<float>());
        }

        public static JObject FromQuaternion(Quaternion q)
        {
            return new JObject
            {
                ["x"] = q.x,
                ["y"] = q.y,
                ["z"] = q.z,
                ["w"] = q.w,
            };
        }

        public static Quaternion ToQuaternion(JObject o)
        {
            return new Quaternion(
                o["x"].ToObject<float>(),
                o["y"].ToObject<float>(),
                o["z"].ToObject<float>(),
                o["w"].ToObject<float>());
        }
    }

    public struct ChudRigTransform
    {
        public Vector3 HeadPosition;
        public Quaternion HeadRotation;

        public Vector3 RigPosition;
        public Quaternion RigRotation;

        public Vector3 LeftHandPosition;
        public Quaternion LeftHandRotation;

        public Vector3 RightHandPosition;
        public Quaternion RightHandRotation;

        public Vector3 Velocity;

        public ChudRigTransform(Vector3 headPos, Quaternion headRot, Vector3 rigPos, Quaternion rigRot,
            Vector3 leftPos, Quaternion leftRot, Vector3 rightPos, Quaternion rightRot, Vector3 velocity)
        {
            HeadPosition = headPos;
            HeadRotation = headRot;
            RigPosition = rigPos;
            RigRotation = rigRot;
            LeftHandPosition = leftPos;
            LeftHandRotation = leftRot;
            RightHandPosition = rightPos;
            RightHandRotation = rightRot;
            Velocity = velocity;
        }

        public static ChudRigTransform GetRigPosition(VRRig rig)
        {
            if (rig == null)
                return default(ChudRigTransform);

            Vector3 headPos = rig.head != null && rig.head.rigTarget != null
                ? rig.head.rigTarget.position
                : rig.transform.position + Vector3.up * 1.6f;
            Quaternion headRot = rig.head != null && rig.head.rigTarget != null
                ? rig.head.rigTarget.rotation
                : rig.transform.rotation;

            Vector3 rigPos = rig.transform.position;
            Quaternion rigRot = rig.transform.rotation;

            Vector3 leftPos = rig.leftHand != null && rig.leftHand.rigTarget != null
                ? rig.leftHand.rigTarget.position
                : rigPos;
            Quaternion leftRot = rig.leftHand != null && rig.leftHand.rigTarget != null
                ? rig.leftHand.rigTarget.rotation
                : rigRot;

            Vector3 rightPos = rig.rightHand != null && rig.rightHand.rigTarget != null
                ? rig.rightHand.rigTarget.position
                : rigPos;
            Quaternion rightRot = rig.rightHand != null && rig.rightHand.rigTarget != null
                ? rig.rightHand.rigTarget.rotation
                : rigRot;

            Vector3 vel = Vector3.zero;
            try
            {
                if (rig.isLocal && GorillaTagger.Instance != null && GorillaTagger.Instance.rigidbody != null)
                    vel = GorillaTagger.Instance.rigidbody.linearVelocity;
            }
            catch { }

            return new ChudRigTransform(headPos, headRot, rigPos, rigRot, leftPos, leftRot, rightPos, rightRot, vel);
        }

        public JObject ToJObject()
        {
            return new JObject
            {
                ["headPosition"] = MacroJsonHelpers.FromVector3(HeadPosition),
                ["headRotation"] = MacroJsonHelpers.FromQuaternion(HeadRotation),
                ["rigPosition"] = MacroJsonHelpers.FromVector3(RigPosition),
                ["rigRotation"] = MacroJsonHelpers.FromQuaternion(RigRotation),
                ["leftHandPosition"] = MacroJsonHelpers.FromVector3(LeftHandPosition),
                ["leftHandRotation"] = MacroJsonHelpers.FromQuaternion(LeftHandRotation),
                ["rightHandPosition"] = MacroJsonHelpers.FromVector3(RightHandPosition),
                ["rightHandRotation"] = MacroJsonHelpers.FromQuaternion(RightHandRotation),
                ["velocity"] = MacroJsonHelpers.FromVector3(Velocity),
            };
        }

        public static ChudRigTransform FromJObject(JObject o)
        {
            return new ChudRigTransform(
                MacroJsonHelpers.ToVector3((JObject)o["headPosition"]),
                MacroJsonHelpers.ToQuaternion((JObject)o["headRotation"]),
                MacroJsonHelpers.ToVector3((JObject)o["rigPosition"]),
                MacroJsonHelpers.ToQuaternion((JObject)o["rigRotation"]),
                MacroJsonHelpers.ToVector3((JObject)o["leftHandPosition"]),
                MacroJsonHelpers.ToQuaternion((JObject)o["leftHandRotation"]),
                MacroJsonHelpers.ToVector3((JObject)o["rightHandPosition"]),
                MacroJsonHelpers.ToQuaternion((JObject)o["rightHandRotation"]),
                MacroJsonHelpers.ToVector3((JObject)o["velocity"]));
        }
    }

    public struct ChudMacro
    {
        public System.Collections.Generic.List<ChudRigTransform> Positions;
        public string Name;

        public readonly string DumpJson()
        {
            JObject obj = new JObject
            {
                ["name"] = Name,
                ["positions"] = new JArray(Positions.ConvertAll(p => p.ToJObject())),
            };
            return obj.ToString();
        }

        public static ChudMacro LoadJson(string json)
        {
            JObject obj = JObject.Parse(json);
            ChudMacro macro = new ChudMacro
            {
                Name = (string)obj["name"],
                Positions = new System.Collections.Generic.List<ChudRigTransform>(),
            };
            foreach (JToken token in (JArray)obj["positions"])
                macro.Positions.Add(ChudRigTransform.FromJObject((JObject)token));
            return macro;
        }
    }
}
