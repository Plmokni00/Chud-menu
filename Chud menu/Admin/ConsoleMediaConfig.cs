using System;
using System.Collections.Generic;
using System.IO;
using Chud.UI;
using Newtonsoft.Json;
using UnityEngine;
using static Chud.Backend.ConsoleMods;

namespace Chud.Backend;

public class MediaEntry
{
	public string Name;
	public string Url;
}

public static class ConsoleMediaConfig
{
	public static readonly string FilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), WristMenu.FolderName, "ConsoleMediaConfig.json");

	private static List<MediaEntry> _sounds;
	private static List<MediaEntry> _videos;
	private static bool _loaded;

	public static bool IsLoaded => _loaded;
	public static IReadOnlyList<MediaEntry> Sounds => _sounds ?? (IReadOnlyList<MediaEntry>)Array.Empty<MediaEntry>();
	public static IReadOnlyList<MediaEntry> Videos => _videos ?? (IReadOnlyList<MediaEntry>)Array.Empty<MediaEntry>();

	public static string GetSoundName(int index)
	{
		if (_loaded && _sounds != null && index >= 0 && index < _sounds.Count)
			return _sounds[index].Name;
		if (index >= 0 && index < soundNames.Length)
			return soundNames[index];
		return "Unknown";
	}

	public static string GetVideoName(int index)
	{
		if (_loaded && _videos != null && index >= 0 && index < _videos.Count)
			return _videos[index].Name;
		if (index >= 0 && index < videoNames.Length)
			return videoNames[index];
		return "Unknown";
	}

	public static int SoundCount
	{
		get
		{
			if (_loaded && _sounds != null)
				return _sounds.Count;
			return soundNames.Length;
		}
	}

	public static int VideoCount
	{
		get
		{
			if (_loaded && _videos != null)
				return _videos.Count;
			return videoNames.Length;
		}
	}

	public static string GetSoundUrl(int index)
	{
		if (_loaded && _sounds != null && index >= 0 && index < _sounds.Count)
		{
			string url = _sounds[index].Url;
			if (!string.IsNullOrEmpty(url))
				return url;
		}
		return ConsoleMods.GetSoundUrl(index);
	}

	public static string GetVideoUrl(int index)
	{
		if (_loaded && _videos != null && index >= 0 && index < _videos.Count)
		{
			string url = _videos[index].Url;
			if (!string.IsNullOrEmpty(url))
				return url;
		}
		return ConsoleMods.GetVideoUrl(index);
	}

	public static void WriteConfig()
	{
		try
		{
			string dir = Path.GetDirectoryName(FilePath);
			if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var data = new MediaConfigData
			{
				sounds = new MediaEntry[soundNames.Length],
				videos = new MediaEntry[videoNames.Length]
			};

			for (int i = 0; i < soundNames.Length; i++)
				data.sounds[i] = new MediaEntry { Name = soundNames[i], Url = ConsoleMods.GetSoundUrl(i) };

			for (int i = 0; i < videoNames.Length; i++)
				data.videos[i] = new MediaEntry { Name = videoNames[i], Url = ConsoleMods.GetVideoUrl(i) };

			File.WriteAllText(FilePath, JsonConvert.SerializeObject(data, Formatting.Indented));
		}
		catch
		{
		}
	}

	public static void LoadConfig()
	{
		try
		{
			if (!File.Exists(FilePath))
			{
				_loaded = false;
				return;
			}
			string json = File.ReadAllText(FilePath);
			var data = JsonConvert.DeserializeObject<MediaConfigData>(json);
			if (data != null)
			{
				_sounds = new List<MediaEntry>(data.sounds ?? Array.Empty<MediaEntry>());
				_videos = new List<MediaEntry>(data.videos ?? Array.Empty<MediaEntry>());
				_loaded = (_sounds.Count > 0 || _videos.Count > 0);
			}
		}
		catch
		{
			_loaded = false;
		}
	}

	private class MediaConfigData
	{
		public MediaEntry[] sounds;
		public MediaEntry[] videos;
	}
}
