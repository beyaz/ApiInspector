using System.IO;
using ReactWithDotNet.Libraries.PrimeReact;

namespace ApiInspector.WebUI.Components;

public class AssemblySelector : ReactComponent
{
    public string AssemblyDirectoryPath { get; set; }

    public string AssemblyFileName { get; set; }

    public string Query { get; set; }

    [ReactCustomEvent]
    public Action<string> SelectionChanged { get; set; }

    protected override Element render()
    {
        var suggestions = Enumerable.Empty<string>();

        if (Directory.Exists(AssemblyDirectoryPath))
        {
            suggestions = Directory.GetFiles(AssemblyDirectoryPath).Where(x => Path.GetExtension(x) == ".dll" || Path.GetExtension(x) == ".exe").Where(x => x?.Contains(Query ?? string.Empty, StringComparison.OrdinalIgnoreCase) == true).Select(Path.GetFileName).Take(7);
        }

        return new AutoComplete<string>
               {
                   value = AssemblyFileName,

                   suggestions    = suggestions,
                   completeMethod = _ => Query = _.query,
                   onChange       = OnChange,
                   inputStyle     = { WidthMaximized }
               }
               + FlexGrow(1);
    }

    void OnChange(AutoCompleteChangeParams<string> e)
    {
        AssemblyFileName = e.value;

        if (File.Exists(Path.Combine(AssemblyDirectoryPath, AssemblyFileName)))
        {
            DispatchEvent(() => SelectionChanged, AssemblyFileName);
        }
    }
}