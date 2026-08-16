global using static ApiInspector.Mixin;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ApiInspector;

static class Mixin
{
    internal static void WaitForDebuggerAttach()
    {
        while (!Debugger.IsAttached)
        {
            Thread.Sleep(100);
        }
    }
}

sealed class LogTextWriter : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string value)
    {
        WriteLog(value);
    }

    public override void WriteLine(string value)
    {
        WriteLog(value);
    }
}