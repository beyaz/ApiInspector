using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;

namespace ApiInspector.WebUI;

static class ReactWithDotNetIntegration
{
    public static void ConfigureReactWithDotNet(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", HomePage);
        endpoints.MapPost("/" + nameof(HandleReactWithDotNetRequest), HandleReactWithDotNetRequest);
    }

    static Task HandleReactWithDotNetRequest(HttpContext httpContext)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        return ProcessReactWithDotNetComponentRequest(new ProcessReactWithDotNetComponentRequestInput
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

        return ProcessReactWithDotNetPageRequest(new ProcessReactWithDotNetPageRequestInput
        {
            LayoutType      = layoutType,
            MainContentType = mainContentType,
            HttpContext     = httpContext,
            QueryString     = httpContext.Request.QueryString.ToString()
        });
    }
}