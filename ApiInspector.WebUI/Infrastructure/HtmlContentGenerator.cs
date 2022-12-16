using System.Text;

namespace ApiInspector.WebUI;

sealed class HtmlContentGenerator
{
    public Type TargetReactComponent { get; set; }

    public string GetHtmlContent()
    {
        const string root = "wwwroot";

        var lines = new List<Line>
        {
            "<!DOCTYPE html>",

            "<html lang='en' xmlns='http://www.w3.org/1999/xhtml' dir='ltr'>",

            "<head>",
            "    <meta charset='utf-8' />",
            "    <meta name='viewport' content='width=device-width, initial-scale=1'>",

            "    <meta http-equiv='Cache-Control' content='no-cache, no-store, must-revalidate' />",
            "    <meta http-equiv='Pragma' content='no-cache' />",
            "    <meta http-equiv='Expires' content='0' />",

            "    <title>Api Inspector</title>",

            "    <!-- Font -->",
            "    <link rel='stylesheet' href='https://fonts.googleapis.com/css?family=Nunito+Sans:400,700,800,900&amp;display=swap' media='all'>",

            "    <!-- Styles -->",
            "    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/primereact@8.2.0/resources/primereact.min.css'>",
            "    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/primeicons@5.0.0/primeicons.min.css'>",
            "    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/primereact@8.2.0/resources/themes/saga-blue/theme.min.css'>",

            "</head>",

            "<body>",
            "    <div id='app'>",
            $"        <script src='{root}/index.js'></script>",
            "    </div>",
            "</body>",

            "</html>",

            "<script type='text/javascript'>",
            "    ReactWithDotNet.RenderComponentIn({",
            $"        fullTypeNameOfReactComponent: '{TargetReactComponent.GetFullName()}',",
            "        containerHtmlElementId: 'app'",
            "    });",
            "</script>",
            "",

            "",
            @"
               <style>
                   html, body {
                       height: 100vh;
                       margin: 0;
                       font-family: 'Nunito Sans', 'Helvetica Neue', Helvetica, Arial, sans-serif;
                       font-size: 13px;
                       color: rgb(51, 51, 51);
                   }
                   
                   #app {
                       width: 100%;
                       height: 100vh;
                   }
                   
                   input:focus, textarea:focus, select:focus {
                       outline: none;
                   }
               </style>
              "
        };

        return lines.Aggregate(new StringBuilder(), (sb, line) => line.WriteTo(sb)).ToString();
    }

    class Line
    {
        string value;

        string[] values;

        public static implicit operator Line(string line)
        {
            return new Line { value = line };
        }

        public static implicit operator Line(string[] lines)
        {
            return new Line { values = lines };
        }

        public StringBuilder WriteTo(StringBuilder sb)
        {
            if (values?.Length > 0)
            {
                foreach (var item in values)
                {
                    sb.AppendLine(item);
                }

                return sb;
            }

            sb.AppendLine(value);

            return sb;
        }
    }
}