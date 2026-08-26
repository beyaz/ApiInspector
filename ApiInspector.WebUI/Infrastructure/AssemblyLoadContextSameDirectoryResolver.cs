using System.Reflection;
using System.Runtime.Loader;

namespace ApiInspector.WebUI;

static class AssemblyLoadContextSameDirectoryResolver
{
    internal static void AttachResolver()
    {
        AssemblyLoadContext.Default.Resolving += (_, name) => Resolve(name, Path.GetDirectoryName(typeof(AssemblyLoadContextSameDirectoryResolver).Assembly.Location));
        return;

        static Assembly Resolve(AssemblyName name, string baseDirectory)
        {
            if (name?.Name == null)
            {
                return null;
            }

            // Eğer zaten yüklüyse onu kullan
            var already = AppDomain.CurrentDomain.GetAssemblies()
                                   .FirstOrDefault(a => string.Equals(a.GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase));
            if (already != null)
            {
                return already;
            }

            var assemblyPath = Path.Combine(baseDirectory, name.Name + ".dll");
            if (!File.Exists(assemblyPath))
            {
                return null;
            }

            // .NET Core: LoadFromAssemblyPath kullanmak load-context problemlerini azaltır
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));
        }
    }
}