using UnityEngine;

namespace Chud.Backend;

public static class RPCProtection
{
	const int MaxRPCs = 500;
	static readonly float[] eventTimes = new float[MaxRPCs];
	static int head = 0;
	static int count = 0;
	static readonly object _lock = new object();

	public static bool Prefix()
	{
		lock (_lock)
		{
			float now = Time.unscaledTime;
			while (count > 0 && now - eventTimes[head] > 1f)
			{
				head = (head + 1) % MaxRPCs;
				count--;
			}
			if (count >= MaxRPCs) return false;
			eventTimes[(head + count) % MaxRPCs] = now;
		count++;
		return true;
		}
	}
}
