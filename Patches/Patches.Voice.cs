using Chud.Diagnostics;
using HarmonyLib;
using Photon.Voice;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
namespace Chud.Patches
{
    public static class PhotonVoiceResolver
    {
        public const float LogThrottleSeconds = 5f;

        public static Type TransportProtocol => Cached("Photon.Voice.PhotonTransportProtocol");

        public static Type LoadBalancingTransport => Cached("Photon.Voice.LoadBalancingTransport");

        public static Type VoiceConnection => Cached("Photon.Voice.Unity.VoiceConnection");

        public static FieldInfo VoiceClientField => _voiceClientField ?? (_voiceClientField =
            AccessTools.Field(TransportProtocol, "voiceClient"));

        private static FieldInfo _voiceClientField;

        private static readonly Dictionary<string, MethodBase> MethodCache = new Dictionary<string, MethodBase>(8);

        public static MethodBase Find(string typeName, string methodName, Type[] parameters)
        {
            string key = typeName + "." + methodName + "/" + (parameters?.Length ?? -1);
            if (MethodCache.TryGetValue(key, out MethodBase cached) && cached != null)
            {
                return cached;
            }

            Type type = Cached(typeName);
            MethodBase method = type == null
                ? null
                : parameters == null
                    ? AccessTools.Method(type, methodName)
                    : AccessTools.Method(type, methodName, parameters);

            if (method == null)
            {
                Log.Warn("Photon voice member '" + key + "' not found; that voice fix will be inactive");
            }

            MethodCache[key] = method;
            return method;
        }

        private static Type Cached(string fullName)
        {
            if (TypeCache.TryGetValue(fullName, out Type cached) && cached != null)
            {
                return cached;
            }

            Type found = AccessTools.TypeByName(fullName);
            if (found == null)
            {
                Log.Warn("Photon voice type '" + fullName + "' not found");
            }

            TypeCache[fullName] = found;
            return found;
        }

        private static readonly Dictionary<string, Type> TypeCache = new Dictionary<string, Type>(4);
    }

    public static class ThrottledLogger
    {
        private static readonly Dictionary<string, float> Last = new Dictionary<string, float>(8);

        public static void Warn(string key, string message, float now)
        {
            if (Last.TryGetValue(key, out float previous) && now - previous < PhotonVoiceResolver.LogThrottleSeconds)
            {
                Last[key] = now;
                return;
            }

            Last[key] = now;
            UnityEngine.Debug.LogWarning("[Chud] " + message);
        }
    }

    [HarmonyPatch]
    internal static class VoiceFixOnVoiceInfo
    {
        private const byte KeyVoiceId = 1;
        private const byte KeyEventNumber = 11;

        private static MethodBase TargetMethod()
        {
            return PhotonVoiceResolver.Find("Photon.Voice.PhotonTransportProtocol", "onVoiceInfo",
                new[] { typeof(int), typeof(int), typeof(object) });
        }

        private static bool Prefix(object __instance, int channelId, int playerId, object payload)
        {
            var array = payload as object[];
            if (array == null)
            {
                return false;
            }

            VoiceClient client = PhotonVoiceResolver.VoiceClientField?.GetValue(__instance) as VoiceClient;
            if (client == null)
            {
                return false;
            }

            for (int i = 0; i < array.Length; i++)
            {
                Dispatch(client, array[i], channelId, playerId);
            }

            return false;
        }

        private static void Dispatch(VoiceClient client, object entry, int channelId, int playerId)
        {
            try
            {
                Dictionary<byte, object> fields = ToByteMap(entry);
                if (fields == null || !fields.ContainsKey(KeyVoiceId) || !fields.ContainsKey(KeyEventNumber))
                {
                    return;
                }

                byte voiceId = ToByte(fields[KeyVoiceId]);
                byte eventNumber = ToByte(fields[KeyEventNumber]);

                client.onVoiceInfo(channelId, playerId, voiceId, eventNumber, BuildVoiceInfo(fields));
            }
            catch (Exception ex)
            {
                ThrottledLogger.Warn("voiceInfo", "voice packet dispatch failed: " + ex.Message,
                    UnityEngine.Time.realtimeSinceStartup);
            }
        }

        private static Dictionary<byte, object> ToByteMap(object entry)
        {
            if (entry is Dictionary<byte, object> direct)
            {
                return direct;
            }

            if (!(entry is IDictionary source))
            {
                return null;
            }

            var map = new Dictionary<byte, object>(source.Count);
            foreach (DictionaryEntry pair in source)
            {
                map[ToByte(pair.Key)] = pair.Value;
            }

            return map;
        }

        private static VoiceInfo BuildVoiceInfo(Dictionary<byte, object> fields)
        {
            var info = new VoiceInfo();

            if (fields.TryGetValue(12, out object codec))
            {
                info.Codec = codec is Codec typed ? typed : (Codec)ToInt(codec);
            }

            if (fields.TryGetValue(2, out object sampling)) info.SamplingRate = ToInt(sampling);
            if (fields.TryGetValue(3, out object channels)) info.Channels = ToInt(channels);
            if (fields.TryGetValue(4, out object frame)) info.FrameDurationUs = ToInt(frame);
            if (fields.TryGetValue(5, out object bitrate)) info.Bitrate = ToInt(bitrate);
            if (fields.TryGetValue(6, out object width)) info.Width = ToInt(width);
            if (fields.TryGetValue(7, out object height)) info.Height = ToInt(height);
            if (fields.TryGetValue(8, out object fps)) info.FPS = ToInt(fps);
            if (fields.TryGetValue(9, out object keyFrame)) info.KeyFrameInt = ToInt(keyFrame);
            if (fields.TryGetValue(10, out object userData)) info.UserData = userData;

            return info;
        }

        private static byte ToByte(object value)
        {
            switch (value)
            {
                case byte b: return b;
                case sbyte sb: return (byte)sb;
                case short s: return (byte)s;
                case int i: return (byte)i;
                case long l: return (byte)l;
                case string str when byte.TryParse(str, out byte parsed): return parsed;
            }

            try
            {
                return Convert.ToByte(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private static int ToInt(object value)
        {
            switch (value)
            {
                case int i: return i;
                case short s: return s;
                case ushort us: return us;
                case byte b: return b;
                case sbyte sb: return sb;
                case long l: return (int)l;
                case string str when int.TryParse(str, out int parsed): return parsed;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }

    [HarmonyPatch]
    internal static class VoiceFixOnVoiceEvent
    {
        private static MethodBase TargetMethod()
        {
            return PhotonVoiceResolver.Find("Photon.Voice.PhotonTransportProtocol", "onVoiceEvent",
                new[] { typeof(object), typeof(int), typeof(int), typeof(bool) });
        }

        private static Exception Finalizer(Exception __exception)
        {
            if (__exception == null)
            {
                return null;
            }

            if (__exception is InvalidCastException)
            {
                ThrottledLogger.Warn("voiceEvent", "onVoiceEvent InvalidCast suppressed",
                    UnityEngine.Time.realtimeSinceStartup);
                return null;
            }

            return __exception;
        }
    }

    [HarmonyPatch]
    internal static class VoiceFixLoadBalancingEvent
    {
        private static MethodBase TargetMethod()
        {
            return PhotonVoiceResolver.Find("Photon.Voice.LoadBalancingTransport", "onEventActionVoiceClient", null);
        }

        private static Exception Finalizer(Exception __exception)
        {
            if (__exception == null)
            {
                return null;
            }

            if (__exception is InvalidCastException)
            {
                ThrottledLogger.Warn("lbEvent", "LoadBalancingTransport.onEventActionVoiceClient suppressed",
                    UnityEngine.Time.realtimeSinceStartup);
                return null;
            }

            return __exception;
        }
    }

    [HarmonyPatch]
    internal static class VoiceFixDispatch
    {
        private static MethodBase TargetMethod()
        {
            return PhotonVoiceResolver.Find("Photon.Voice.Unity.VoiceConnection", "Dispatch", null);
        }

        private static Exception Finalizer(Exception __exception)
        {
            if (__exception is InvalidCastException)
            {
                return null;
            }

            return __exception;
        }
    }
}