using System;
using Chud.Managers;

namespace Chud.Classes;

public class ButtonInfo
{
	public string buttonText = "Error";

	public string overlapText;

	public string[] aliases;

	public string toolTip = "This button doesn't have a tooltip/tutorial";

	public Action method;

	public Action enableMethod;

	public Action disableMethod;

	public bool? enabled = false;

	public ButtonType type = ButtonType.Toggle;

	public bool isTogglable = true;

	public bool legal;

	public bool incremental;

	public bool isSetting;

	public bool hideFromArraylist;

	public bool excludeFromSave;

	public string requiredGameMode;

	public bool requiresLobby;

	public object value;

	public Action onValueChanged;

	public Action<bool> cycleValue;

	public T GetValue<T>()
	{
		if (value == null) return default;
		Type targetType = typeof(T);
		try
		{
			if (targetType.IsEnum)
			{
				return value is string s
					? (T)Enum.Parse(targetType, s, ignoreCase: true)
					: (T)Enum.ToObject(targetType, value);
			}
			if (value is T t) return t;
			if (value is string str && targetType == typeof(int) && int.TryParse(str, out var iv)) return (T)(object)iv;
			if (value is int iv2 && targetType == typeof(string)) return (T)(object)iv2.ToString();
			return (T)Convert.ChangeType(value, targetType);
		}
		catch (Exception e)
		{
			LogManager.Log($"GetValue<{targetType.Name}> failed: value was '{value}' (actual type: {value?.GetType().Name}). {e.Message}");
			throw;
		}
	}

	public void SetValue<T>(T v) => value = v;

	public void SetEnabled(bool value, bool save = true)
	{
		enabled = value;
	}
}
