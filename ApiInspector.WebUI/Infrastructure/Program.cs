using System.Diagnostics;
using System.IO.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApiInspector.WebUI;

public class Program
{
    public static void Main(string[] args)
    {
        ProcessHelper.KillAllNamedProcess("ApiInspector.WebUI");

        var Port = NetworkHelper.GetAvailablePort(Config.NextAvailablePortFrom);

        if (Config.HideConsoleWindow)
        {
            IgnoreException(ConsoleWindowUtility.HideConsoleWindow);
        }

        if (Config.UseUrls)
        {
            Process.Start(Config.BrowserExePath, Config.BrowserExeArguments.Replace("{Port}", Port.ToString()));
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
            app.Run($"https://*:{Port}");
        }
        else
        {
            app.Run();
        }
    }
}