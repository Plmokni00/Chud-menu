namespace Chud.Menu
{
    public enum ButtonType
    {
        Action,

        Toggle,

        FrameToggle,

        Gun
    }

    public static class ReservedButtonIds
    {
        public const string NextPage = "NextPage";
        public const string PreviousPage = "PreviousPage";
        public const string Disconnect = "DisconnectingButton";

        public const string Title = "title";
        public const string Status = "status";
        public const string ConsoleEntry = "main_console_mods";
    }
}