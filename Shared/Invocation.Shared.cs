global using static ApiInspector.Mixin;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ApiInspector;

#if NETCOREAPP
using System.Runtime.Loader;
#endif

static class AssemblyResolver
{
    static readonly object Sync = new();
    
    static bool Attached;

    public static void AttachAssemblyResolverForSameDirectory(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new ArgumentException(nameof(baseDirectory));
        }

        baseDirectory = Path.GetFullPath(baseDirectory);

        lock (Sync)
        {
            if (Attached)
            {
                return;
            }

            Attached = true;

            #if NETCOREAPP
            AssemblyLoadContext.Default.Resolving += (_, name) => Resolve(name, baseDirectory);
            #else
            AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
            {
                var assemblyName = new AssemblyName(e.Name);
                
                return Resolve(assemblyName, baseDirectory);
            };
            #endif
        }
    }

    #if NETCOREAPP
    static Assembly Resolve(AssemblyName name, string baseDirectory)
    {
        if (name?.Name == null) return null;

        // Eğer zaten yüklüyse onu kullan
        var already = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => string.Equals(a.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase));
        if (already != null) return already;

        var assemblyPath = Path.Combine(baseDirectory, name.Name + ".dll");
        if (!File.Exists(assemblyPath)) return null;

        // .NET Core: LoadFromAssemblyPath kullanmak load-context problemlerini azaltır
        return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
    }
    #else
    static Assembly Resolve(AssemblyName name, string baseDirectory)
    {
        if (name?.Name == null)
        {
            return null;
        }

        var already = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => string.Equals(a.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase));
        if (already != null)
        {
            return already;
        }

        var assemblyPath = Path.Combine(baseDirectory, name.Name + ".dll");
        if (!File.Exists(assemblyPath))
        {
            return null;
        }

        return Assembly.LoadFrom(Path.GetFullPath(assemblyPath));
    }
    #endif
}

static class Mixin
{
    internal static void AttachAssemblyResolverForSameDirectory(string baseDirectory)
    {
        // A t t a c h   A s s e m b l y   R e s o l v e r   F o r   S a m e    D i r e c t o r y 
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, e) =>
            {
                var assemblyName = new AssemblyName(e.Name).Name;
                var assemblyPath = Path.Combine(baseDirectory, assemblyName + ".dll");
                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

                return null;
            };
        }
    }

    internal static string LocalIPAddress()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            return null;
        }

        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();
    }

    internal static IReadOnlyDictionary<TKey, TValue> NewDictionaryFrom<TKey, TValue>(IEnumerable<(TKey name, TValue value)> items)
    {
        var map = new Dictionary<TKey, TValue>();
        foreach (var (name, value) in items)
        {
            map[name] = value;
        }

        return map;
    }

    internal static object TryCreateAsValueType(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

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