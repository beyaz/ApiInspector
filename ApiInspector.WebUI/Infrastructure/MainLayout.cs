namespace ApiInspector.WebUI;

sealed class MainLayout : PureComponent, IPageLayout
{
    public string ContainerDomElementId => "app";

    // After page first rendered in client then connect with react system in background.
    // So user first iteraction time will be minimize.
    public string InitialScript
        => $$"""
             import {ReactWithDotNet} from '{{IndexJsFilePath}}';

             ReactWithDotNet.StrictMode = false;

             ReactWithDotNet.RequestHandlerPath = '{{RequestHandlerPath}}';

             ReactWithDotNet.RenderComponentIn({
               idOfContainerHtmlElement: '{{ContainerDomElementId}}',
               renderInfo: {{RenderInfo.ToJsonString()}}
             });
             """;

    public ComponentRenderInfo RenderInfo { get; set; }

    protected override Element render()
    {
        var root = Context.wwwroot;

        return new html
        {
            Lang("tr"),
            DirLtr,

            new head
            {
                new meta { charset = "utf-8" },
                new meta { name    = "viewport", content = "width=device-width, initial-scale=1" },
                new title { "Api Inspector" },
                new link { rel = "icon", href = $"{root}/favicon.ico" },

                new link
                {
                    rel  = "stylesheet",
                    type = "text/css",
                    href = IndexCssFilePath
                },

                new style
                {
                    """

                    * {
                        margin: 0;
                        padding: 0;
                        box-sizing: border-box;
                    }
                       
                    html, body {
                        height: 100vh;
                        margin: 0;
                        font-family: 'Nunito Sans', 'Helvetica Neue', Helvetica, Arial, sans-serif;
                        font-size: 13px;
                        color: rgb(51, 51, 51);
                    }
                       
                    input:focus, textarea:focus, select:focus {
                        outline: none;
                    }

                    """
                },

                new link
                {
                    rel  = "stylesheet",
                    href = "https://fonts.googleapis.com/css?family=Nunito+Sans:400,700,800,900&amp;display=swap", media = "all"
                },
                new link
                {
                    href = "https://fonts.cdnfonts.com/css/ibm-plex-mono-3",
                    rel  = "stylesheet"
                }
            },
            new body
            {
                new div(Id(ContainerDomElementId), SizeFull)
            }
        };
    }
}