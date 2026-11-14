namespace PersonalWiki.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // ApplicationConfiguration.Initialize() applies PerMonitorV2 from the
        // Desktop project setting before any WinForms handle is created.
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
