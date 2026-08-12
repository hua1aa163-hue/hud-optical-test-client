namespace HudOpticalTestClient;

/// <summary>HUD 光学测试客户端入口。</summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
