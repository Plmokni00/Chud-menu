using System;

namespace Chud.Managers;

public static class LogManager
{
	public static void Log(object message)
	{
		UnityEngine.Debug.Log($"[Chud] {message}");
	}

	public static void LogError(object message)
	{
		UnityEngine.Debug.LogError($"[Chud] {message}");
	}

	public static void LogWarning(object message)
	{
		UnityEngine.Debug.LogWarning($"[Chud] {message}");
	}
}
