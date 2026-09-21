using System;

namespace Chud.Classes;

public class ButtonInfo
{
	public string id;

	public string buttonText = "Error";

	public string toolTip = "This button doesn't have a tooltip/tutorial";

	public Action method;

	public Action enableMethod;

	public Action disableMethod;

	public bool? enabled = false;

	public ButtonType type = ButtonType.Toggle;

	public string requiredGameMode;

	public bool requiresLobby;
}
