using System;

namespace Chud.Classes;

public class ButtonInfo
{
	public string buttonText = "Error";

	public string toolTip = "This button doesn't have a tooltip/tutorial";

	public Action method;

	public Action enableMethod;

	public Action disableMethod;

	public bool? enabled = false;

	public ButtonType type = ButtonType.Toggle;

	public bool isTogglable = true;

	public string requiredGameMode;

	public bool requiresLobby;
}
