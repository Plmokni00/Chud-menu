using System;
using System.Collections;
using System.IO;
using System.Text;
using Chud.UI;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace Chud.Backend;

public static class SoundCache
{
	public static string CacheFolder => Path.Combine(WristMenu.FolderName, "Cache");

	public static string CachePathForUrl(string url)
	{
		string ext = ".mp3";
		try
		{
			string lower = url.ToLower();
			int cut = lower.IndexOf('?');
			if (cut >= 0) lower = lower.Substring(0, cut);
			if (lower.EndsWith(".wav")) ext = ".wav";
			else if (lower.EndsWith(".ogg")) ext = ".ogg";
			else if (lower.EndsWith(".mp3")) ext = ".mp3";
		}
		catch { }
		StringBuilder sb = new StringBuilder(64);
		try
		{
			foreach (char c in url)
			{
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
					sb.Append(c);
				else
					sb.Append('_');
				if (sb.Length >= 48) break;
			}
		}
		catch { }
		if (sb.Length == 0) sb.Append("clip");
		return Path.Combine(CacheFolder, sb.ToString() + ext);
	}

	public static AudioType AudioTypeForUrl(string url)
	{
		try
		{
			string lower = url.ToLower();
			if (lower.Contains(".wav")) return AudioType.WAV;
			if (lower.Contains(".ogg")) return AudioType.OGGVORBIS;
		}
		catch { }
		return AudioType.MPEG;
	}

	public static IEnumerator GetClip(string url, Action<AudioClip> onDone)
	{
		AudioType audioType = AudioTypeForUrl(url);
		string path = "";
		try { path = CachePathForUrl(url); } catch { path = ""; }
		if (!string.IsNullOrEmpty(path))
		{
			bool diskHit = false;
			try { diskHit = File.Exists(path); } catch { diskHit = false; }
			if (diskHit)
			{
				AudioClip diskClip = null;
				UnityWebRequest diskReq = UnityWebRequestMultimedia.GetAudioClip("file:///" + path.Replace("\\", "/"), audioType);
				try
				{
					yield return diskReq.SendWebRequest();
					if (diskReq.result == UnityWebRequest.Result.Success)
					{
						try { diskClip = DownloadHandlerAudioClip.GetContent(diskReq); } catch { diskClip = null; }
					}
				}
				finally
				{
					try { diskReq.Dispose(); } catch { }
				}
				if ((Object)(object)diskClip != (Object)null)
				{
					try { onDone?.Invoke(diskClip); } catch { }
					yield break;
				}
				try { File.Delete(path); } catch { }
			}
		}
		AudioClip netClip = null;
		byte[] data = null;
		UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
		try
		{
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				try { data = req.downloadHandler != null ? req.downloadHandler.data : null; } catch { data = null; }
				try { netClip = DownloadHandlerAudioClip.GetContent(req); } catch { netClip = null; }
			}
		}
		finally
		{
			try { req.Dispose(); } catch { }
		}
		if (data != null && data.Length > 0 && !string.IsNullOrEmpty(path))
		{
			try
			{
				if (!Directory.Exists(CacheFolder))
					Directory.CreateDirectory(CacheFolder);
				File.WriteAllBytes(path, data);
			}
			catch { }
		}
		try { onDone?.Invoke(netClip); } catch { }
	}
}
