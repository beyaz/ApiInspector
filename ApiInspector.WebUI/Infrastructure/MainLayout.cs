using System.IO;
using System.Text;

namespace ApiInspector.WebUI;

class MainLayout : Component, IPageLayout
{
    public ComponentRenderInfo RenderInfo { get; set; }
    
    public string ContainerDomElementId => "app";

    static readonly string LastWriteTimeOfIndexJsFile = CalculateLastWriteTimeOfIndexJsFile();
    
    static string CalculateLastWriteTimeOfIndexJsFile()
    {
        const string root = "wwwroot";
            
        var directoryName = Path.GetDirectoryName(typeof(MainLayout).Assembly.Location);

        if (Directory.Exists(directoryName))
        {
            var fileInfo = new FileInfo(Path.Combine(directoryName,root,"dist","index.js"));
            if (fileInfo.Exists)
            {
                return fileInfo.LastWriteTime.Ticks.ToString();
            }
        }

        throw new IOException("index.js file not found");
    }
    protected override Element render()
    {
        const string root = "wwwroot";

        return new html
        {
            Lang("tr"),
            DirLtr,

            new head
            {
                new meta{charset = "utf-8"},
                new meta{name    = "viewport", content = "width=device-width, initial-scale=1"},
                new title{ "Api Inspector" },
                new link{rel ="icon" , href = $"{root}/favicon.ico"},
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

                new link{rel ="stylesheet" , href = "https://fonts.googleapis.com/css?family=Nunito+Sans:400,700,800,900&amp;display=swap", media ="all"},
                new link{href = "https://fonts.cdnfonts.com/css/ibm-plex-mono-3", rel = "stylesheet"}
            },
            new body
            {
                new div(Id(ContainerDomElementId), SizeFull),

                // After page first rendered in client then connect with react system in background.
                // So user first iteraction time will be minimize.

                new script(script.Type("module"))
                {
                    calculateInitialScript()
                }


            }
        };
        
        StringBuilder calculateInitialScript()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"import {{ReactWithDotNet}} from './{root}/dist/index.js?v={LastWriteTimeOfIndexJsFile}';");
            sb.AppendLine("ReactWithDotNet.StrictMode = false;");
            
            sb.AppendLine("ReactWithDotNet.RenderComponentIn({");
            sb.AppendLine($"  idOfContainerHtmlElement: '{ContainerDomElementId}',");
            sb.AppendLine("  renderInfo: ");
            sb.Append(RenderInfo.ToJsonString());
            sb.AppendLine("});");
            
            return sb;
        }
    }
}