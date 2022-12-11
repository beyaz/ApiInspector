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
        return new AutoComplete<string>
        {
            value = AssemblyFileName,

            suggestions    = Directory.GetFiles(AssemblyDirectoryPath).Where(x=>Path.GetExtension(x) ==".dll"|| Path.GetExtension(x) == ".exe").Select(Path.GetFileName),
            completeMethod = _ => Query = _.query,
            onChange       = OnChange
        };
    }

    void OnChange(AutoCompleteChangeParams<string> e)
    {
        AssemblyFileName = e.value;

        if (File.Exists(AssemblyDirectoryPath + AssemblyFileName))
        {
            DispatchEvent(() => SelectionChanged, AssemblyFileName);
        }
    }
}