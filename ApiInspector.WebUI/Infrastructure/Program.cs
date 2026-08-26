using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApiInspector.WebUI;

public class Program
{
    static void AttachResolver()
    {
        AssemblyLoadContext.Default.Resolving += (_, name) => Resolve(name, Path.GetDirectoryName(typeof(Program).Assembly.Location));
        
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
    }
    public static void Main(string[] args)
    {
        AttachResolver();
        
        ProcessHelper.KillAllNamedProcess($"{nameof(ApiInspector)}.{nameof(WebUI)}");

        var port = NetworkHelper.GetAvailablePort(Config.NextAvailablePortFrom);

        if (Config.HideConsoleWindow)
        {
            IgnoreException(ConsoleWindowUtility.HideConsoleWindow);
        }

        if (Config.UseUrls)
        {
            Process.Start(Config.BrowserExePath, Config.BrowserExeArguments.Replace("{Port}", port.ToString()));
        }

        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        // C O N F I G U R E     S E R V I C E S
        services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Fastest; });
        services.Configure<GzipCompressionProviderOptions>(options => { options.Level   = CompressionLevel.Optimal; });
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        // C O N F I G U R E     A P P L I C A T I O N
        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = new("/wwwroot"),
            OnPrepareResponse = ctx =>
            {
                var maxAge = TimeSpan.FromMinutes(5).TotalSeconds;

                ctx.Context.Response.Headers.Append("Cache-Control", $" max-age={maxAge},public,immutable");
            }
        });

        app.UseResponseCompression();

        app.ConfigureReactWithDotNet();

        if (Config.UseUrls)
        {
            app.Run($"http://*:{port}");
        }
        else
        {
            app.Run();
        }
    }
}