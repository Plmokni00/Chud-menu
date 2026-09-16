using HarmonyLib;
using Photon.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ExitGames.Client.Photon;

namespace Chud.Backend
{
    [HarmonyPatch]
    internal class VoiceFix_OnVoiceInfo
    {
        static MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("Photon.Voice.PhotonTransportProtocol");
            if (t == null) return null;
            return AccessTools.Method(t, "onVoiceInfo", new Type[] { typeof(int), typeof(int), typeof(object) });
        }

        static bool Prefix(object __instance, int channelId, int playerId, object payload)
        {
            try
            {
                if (payload == null) return false;
                object[] arr = payload as object[];
                if (arr == null) return false;

                var voiceClientField = AccessTools.Field(__instance.GetType(), "voiceClient");
                var loggerField = AccessTools.Field(__instance.GetType(), "logger");
                var voiceClient = voiceClientField?.GetValue(__instance) as VoiceClient;
                if (voiceClient == null) return false;

                foreach (var obj in arr)
                {
                    Dictionary<byte, object> dict = null;
                    if (obj is Dictionary<byte, object> d) dict = d;
                    else if (obj is ExitGames.Client.Photon.Hashtable h)
                    {
                        dict = new Dictionary<byte, object>();
                        foreach (DictionaryEntry kv in h)
                        {
                            byte k = SafeByte(kv.Key);
                            dict[k] = kv.Value;
                        }
                    }
                    else if (obj is System.Collections.Hashtable sh)
                    {
                        dict = new Dictionary<byte, object>();
                        foreach (DictionaryEntry kv in sh)
                        {
                            byte k = SafeByte(kv.Key);
                            dict[k] = kv.Value;
                        }
                    }
                    else if (obj is IDictionary idict)
                    {
                        dict = new Dictionary<byte, object>();
                        foreach (DictionaryEntry kv in idict)
                        {
                            byte k = SafeByte(kv.Key);
                            dict[k] = kv.Value;
                        }
                    }
                    else continue;

                    if (!dict.ContainsKey(1) || !dict.ContainsKey(11)) continue;

                    byte voiceId = SafeByte(dict[1]);
                    byte evNumber = SafeByte(dict[11]);

                    VoiceInfo info = SafeCreateVoiceInfo(dict);
                    try { voiceClient.onVoiceInfo(channelId, playerId, voiceId, evNumber, info); }
                    catch (Exception e2) { Debug.LogWarning($"[VoiceFix] voiceClient.onVoiceInfo failed: {e2.Message}"); }
                }
            }
            catch (Exception e)
            {
                if (Time.realtimeSinceStartup - lastLog > 5f)
                {
                    Debug.LogWarning($"[VoiceFix] onVoiceInfo suppressed: {e.GetType().Name}: {e.Message}");
                    lastLog = Time.realtimeSinceStartup;
                }
            }
            return false;
        }
        static float lastLog = -999f;

        static byte SafeByte(object o)
        {
            if (o is byte b) return b;
            if (o is sbyte sb) return (byte)sb;
            if (o is int i) return (byte)i;
            if (o is short s) return (byte)s;
            if (o is long l) return (byte)l;
            if (o is string str && byte.TryParse(str, out var pb)) return pb;
            try { return Convert.ToByte(o); } catch { return 0; }
        }
        static int SafeInt(object o)
        {
            if (o is int i) return i;
            if (o is short s) return s;
            if (o is ushort us) return us;
            if (o is byte b) return b;
            if (o is sbyte sb) return sb;
            if (o is long l) return (int)l;
            if (o is string str && int.TryParse(str, out var pi)) return pi;
            try { return Convert.ToInt32(o); } catch { return 0; }
        }
        static VoiceInfo SafeCreateVoiceInfo(Dictionary<byte, object> h)
        {
            VoiceInfo r = new VoiceInfo();
            
            if (h.TryGetValue(12, out var cObj))
            {
                if (cObj is Codec cc) r.Codec = cc;
                else if (cObj is int ci) r.Codec = (Codec)ci;
                else if (cObj is byte cb) r.Codec = (Codec)cb;
                else try { r.Codec = (Codec)Convert.ToInt32(cObj); } catch { r.Codec = Codec.AudioOpus; }
            }
            if (h.TryGetValue(2, out var v)) r.SamplingRate = SafeInt(v);
            if (h.TryGetValue(3, out var v3)) r.Channels = SafeInt(v3);
            if (h.TryGetValue(4, out var v4)) r.FrameDurationUs = SafeInt(v4);
            if (h.TryGetValue(5, out var v5)) r.Bitrate = SafeInt(v5);
            if (h.TryGetValue(6, out var v6)) r.Width = SafeInt(v6);
            if (h.TryGetValue(7, out var v7)) r.Height = SafeInt(v7);
            if (h.TryGetValue(8, out var v8)) r.FPS = SafeInt(v8);
            if (h.TryGetValue(9, out var v9)) r.KeyFrameInt = SafeInt(v9);
            if (h.TryGetValue(10, out var v10)) r.UserData = v10;
            return r;
        }
    }

    [HarmonyPatch]
    internal class VoiceFix_OnVoiceEvent
    {
        static MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("Photon.Voice.PhotonTransportProtocol");
            if (t == null) return null;
            return AccessTools.Method(t, "onVoiceEvent", new Type[] { typeof(object), typeof(int), typeof(int), typeof(bool) });
        }
        static Exception Finalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (__exception is InvalidCastException)
            {
                if (Time.realtimeSinceStartup - lastLog > 5f)
                {
                    Debug.LogWarning($"[VoiceFix] onVoiceEvent InvalidCast suppressed");
                    lastLog = Time.realtimeSinceStartup;
                }
                return null;
            }
            return __exception;
        }
        static float lastLog = -999f;
    }

    [HarmonyPatch]
    internal class VoiceFix_LoadBalancingEvent
    {
        static MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("Photon.Voice.LoadBalancingTransport");
            if (t == null) return null;
            return AccessTools.Method(t, "onEventActionVoiceClient");
        }
        static Exception Finalizer(Exception __exception)
        {
            if (__exception == null) return null;
            if (__exception is InvalidCastException)
            {
                if (Time.realtimeSinceStartup - lastLog > 5f)
                {
                    Debug.LogWarning($"[VoiceFix] LoadBalancingTransport.onEventActionVoiceClient suppressed");
                    lastLog = Time.realtimeSinceStartup;
                }
                return null;
            }
            return __exception;
        }
        static float lastLog = -999f;
    }

    [HarmonyPatch]
    internal class VoiceFix_VoiceConnectionDispatch
    {
        static MethodBase TargetMethod()
        {
            var t = AccessTools.TypeByName("Photon.Voice.Unity.VoiceConnection");
            if (t == null) return null;
            return AccessTools.Method(t, "Dispatch");
        }
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is InvalidCastException) return null;
            return __exception;
        }
    }
}
