using System.Runtime.InteropServices;

namespace ApiInspector.WebUI;

static class ConsoleWindowUtility
{
    public static void HideConsoleWindow()
    {
        const int SW_HIDE = 0;

        var handle = GetConsoleWindow();

        ShowWindow(handle, SW_HIDE);
    }

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}