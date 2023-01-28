namespace ApiInspector.WebUI;

class MainLayout : ReactComponent
{
    public Element Page { get; set; }
    public string QueryString { get; set; }

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
                    @"
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
"
                },

                new link{rel ="stylesheet" , href = "https://fonts.googleapis.com/css?family=Nunito+Sans:400,700,800,900&amp;display=swap", media ="all"},

                new link{rel ="stylesheet" , href = "https://cdn.jsdelivr.net/npm/primereact@8.2.0/resources/primereact.min.css"},
                new link{rel ="stylesheet" , href = "https://cdn.jsdelivr.net/npm/primeicons@5.0.0/primeicons.min.css"},
                new link{rel ="stylesheet" , href = "https://cdn.jsdelivr.net/npm/primereact@8.2.0/resources/themes/saga-blue/theme.min.css"},

            },
            new body
            {
                new div(Id("app"), WidthMaximized,Height100vh)
                {
                    Page
                },

                // After page first rendered in client then connect with react system in background.
                // So user first iteraction time will be minimize.
                
                new script{type ="module", src =$"{root}/dist/index.js"},

                new script
                {
                    type ="module",
                    text =
                        $@"
import {{ReactWithDotNet}} from './{root}/dist/index.js';

setTimeout(()=>ReactWithDotNet.ConnectComponentFirstResponseToReactSystem('app', {CalculateJsonText(Page,QueryString)}), 10);
"
                }


            }
        };
    }
}