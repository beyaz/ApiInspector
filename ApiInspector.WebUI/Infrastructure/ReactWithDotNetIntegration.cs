using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using ReactWithDotNet.UIDesigner;

namespace ApiInspector.WebUI;

class AsyncLogger
{
    public const string UrlPath = "/trace";

    public static readonly List<string> logs = new();
    
    public static async Task HandleRequest(HttpContext httpContext)
    {
        var items = await httpContext.Request.ReadFromJsonAsync<string[]>();
        
        logs.AddRange(items);
                
        httpContext.Response.StatusCode = StatusCodes.Status200OK;
                
        await httpContext.Response.Body.FlushAsync();
    }
}

static class ReactWithDotNetIntegration
{
    public static void ConfigureReactWithDotNet(this WebApplication app)
    {
        app.UseMiddleware<ReactWithDotNetJavaScriptFiles>();
        
        RequestHandlerPath = $"/{nameof(HandleReactWithDotNetRequest)}";
        
        app.Use(async (httpContext, next) =>
        {
            var path = httpContext.Request.Path.Value ?? string.Empty;

            if (path == RequestHandlerPath)
            {
                await HandleReactWithDotNetRequest(httpContext);
                return;
            }

            if (path == "/")
            {
                await HomePage(httpContext);
                return;
            }
            
            if (path == AsyncLogger.UrlPath)
            {
                await AsyncLogger.HandleRequest(httpContext);
                
                return;
            }

            #if DEBUG
            if (path == ReactWithDotNetDesigner.UrlPath)
            {
                await WriteHtmlResponse(httpContext, typeof(MainLayout), typeof(ReactWithDotNetDesigner));
                return;
            }
            #endif

            await next();
        });
    }

    static Task HandleReactWithDotNetRequest(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        return ProcessReactWithDotNetComponentRequest(new()
        {
            HttpContext = httpContext
        });
    }

    static Task HomePage(HttpContext httpContext)
    {
        return WriteHtmlResponse(httpContext, typeof(MainLayout), typeof(MainWindow));
    }

    static Task WriteHtmlResponse(HttpContext httpContext, Type layoutType, Type mainContentType)
    {
        httpContext.Response.ContentType = "text/html; charset=UTF-8";

        httpContext.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
        httpContext.Response.Headers[HeaderNames.Expires]      = "0";
        httpContext.Response.Headers[HeaderNames.Pragma]       = "no-cache";

        return ProcessReactWithDotNetPageRequest(new()
        {
            LayoutType      = layoutType,
            MainContentType = mainContentType,
            HttpContext     = httpContext,
        });
    }
}