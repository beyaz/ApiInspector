using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using ReactWithDotNet.UIDesigner;

namespace ApiInspector.WebUI;

static class ReactWithDotNetIntegration
{
    public static void ConfigureReactWithDotNet(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", HomePage);
        endpoints.MapPost("/" + nameof(HandleReactWithDotNetRequest), HandleReactWithDotNetRequest);

        RegisterReactWithDotNetDevelopmentTools(endpoints);
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

    [Conditional("DEBUG")]
    static void RegisterReactWithDotNetDevelopmentTools(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/" + nameof(ReactWithDotNetDesigner), httpContext =>
        {
            ReactWithDotNetDesigner.IsAttached = true;

            return WriteHtmlResponse(httpContext, typeof(MainLayout), typeof(ReactWithDotNetDesigner));
        });
        endpoints.MapGet("/" + nameof(ReactWithDotNetDesignerComponentPreview), httpContext =>
        {
            ReactWithDotNetDesigner.IsAttached = true;

            return WriteHtmlResponse(httpContext, typeof(MainLayout), typeof(ReactWithDotNetDesignerComponentPreview));
        });
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