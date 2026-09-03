using UnityEngine;

namespace Chud.Backend;

public static class ShaderCache
{
	private static Shader _unlit;
	public static Shader Unlit
	{
		get
		{
			if ((Object)(object)_unlit == (Object)null)
				_unlit = Shader.Find("Universal Render Pipeline/Unlit");
			return _unlit;
		}
	}

	private static Shader _guiText;
	public static Shader GuiText
	{
		get
		{
			if ((Object)(object)_guiText == (Object)null)
				_guiText = Shader.Find("GUI/Text Shader");
			return _guiText;
		}
	}

	private static Shader _uber;
	public static Shader Uber
	{
		get
		{
			if ((Object)(object)_uber == (Object)null)
				_uber = Shader.Find("GorillaTag/UberShader");
			return _uber;
		}
	}
}
