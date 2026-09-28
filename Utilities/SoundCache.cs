using System;
using System.Collections;
using System.IO;
using Chud.Diagnostics;
using Chud.UI;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Chud.Backend
{
    public static class SoundCache
    {
        public static string CacheFolder => Path.Combine(WristMenu.FolderName, "Cache");

        public static string CachePathForUrl(string url)
        {
            return Path.Combine(CacheFolder, "clip_" + FullUrlHash(url) + ExtensionFor(url));
        }

        public static AudioType AudioTypeForUrl(string url)
        {
            string lower = SafeLower(url);
            if (lower == null)
            {
                return AudioType.MPEG;
            }

            if (lower.Contains(".wav")) return AudioType.WAV;
            if (lower.Contains(".ogg")) return AudioType.OGGVORBIS;
            return AudioType.MPEG;
        }

        public static IEnumerator GetClip(string url, Action<AudioClip> onDone)
        {
            AudioType audioType = AudioTypeForUrl(url);
            string path = null;

            try
            {
                path = CachePathForUrl(url);
            }
            catch (Exception ex)
            {
                Log.Warn("could not build a cache path for the clip", ex);
            }

            AudioClip resolved = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                UnityWebRequest disk = UnityWebRequestMultimedia.GetAudioClip(
                    "file:///" + path.Replace("\\", "/"), audioType);

                try
                {
                    yield return disk.SendWebRequest();

                    if (disk.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            resolved = DownloadHandlerAudioClip.GetContent(disk);
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("the cached clip could not be decoded", ex);
                        }
                    }
                }
                finally
                {
                    Dispose(disk);
                }

                if (resolved != null)
                {
                    Invoke(onDone, resolved);
                    yield break;
                }

                TryDelete(path);
            }

            byte[] data = null;
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);

            try
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        data = request.downloadHandler != null ? request.downloadHandler.data : null;
                    }
                    catch (Exception)
                    {
                        data = null;
                    }

                    try
                    {
                        resolved = DownloadHandlerAudioClip.GetContent(request);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("the downloaded clip could not be decoded", ex);
                    }
                }
                else
                {
                    Log.Warn("clip download failed (" + request.result + "): " + url);
                }
            }
            finally
            {
                Dispose(request);
            }

            if (data != null && data.Length > 0 && !string.IsNullOrEmpty(path))
            {
                TryWriteToCache(path, data);
            }

            Invoke(onDone, resolved);
        }

        private static void TryWriteToCache(string path, byte[] data)
        {
            try
            {
                if (!Directory.Exists(CacheFolder))
                {
                    Directory.CreateDirectory(CacheFolder);
                }

                File.WriteAllBytes(path, data);
            }
            catch (Exception ex)
            {
                Log.Warn("could not cache the clip to '" + path + "'", ex);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Log.Warn("could not remove the stale cache entry '" + path + "'", ex);
            }
        }

        private static void Dispose(UnityWebRequest request)
        {
            try
            {
                request?.Dispose();
            }
            catch (Exception ex)
            {
                Log.Warn("could not release the web request", ex);
            }
        }

        private static void Invoke(Action<AudioClip> callback, AudioClip clip)
        {
            if (callback == null)
            {
                return;
            }

            try
            {
                callback(clip);
            }
            catch (Exception ex)
            {
                Log.Error("the clip callback threw", ex);
            }
        }

        private static string ExtensionFor(string url)
        {
            string lower = SafeLower(url);
            if (lower == null)
            {
                return ".mp3";
            }

            if (lower.EndsWith(".wav", StringComparison.Ordinal)) return ".wav";
            if (lower.EndsWith(".ogg", StringComparison.Ordinal)) return ".ogg";
            return ".mp3";
        }

        private static string SafeLower(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                return value.ToLowerInvariant();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string FullUrlHash(string url)
        {
            unchecked
            {
                ulong hash = 14695981039346656037ul;
                string source = url ?? string.Empty;

                for (int i = 0; i < source.Length; i++)
                {
                    hash ^= source[i];
                    hash *= 1099511628211ul;
                }

                return hash.ToString("x16");
            }
        }
    }
}
