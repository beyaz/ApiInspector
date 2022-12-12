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

            suggestions    = Directory.GetFiles(AssemblyDirectoryPath).Where(x=>Path.GetExtension(x) ==".dll"|| Path.GetExtension(x) == ".exe").Where(x=>x?.Contains(Query??String.Empty,StringComparison.OrdinalIgnoreCase)==true).Select(Path.GetFileName).Take(7),
            completeMethod = _ => Query = _.query,
            onChange       = OnChange,
            inputStyle     = { WidthMaximized}
        } 
        + FlexGrow(1);
    }

    void OnChange(AutoCompleteChangeParams<string> e)
    {
        AssemblyFileName = e.value;

        if (File.Exists(Path.Combine(AssemblyDirectoryPath , AssemblyFileName)))
        {
            DispatchEvent(() => SelectionChanged, AssemblyFileName);
        }
    }
}

public class DirectorySelector : ReactComponent
{
    public string DirectoryPath{ get; set; }

    public string Query { get; set; }

    [ReactCustomEvent]
    public Action<string> SelectionChanged { get; set; }


    protected override Element render()
    {
      var suggestions = Enumerable.Empty<string>();
        
        if (Directory.Exists(DirectoryPath))
        {
            suggestions = Directory.GetDirectories(DirectoryPath);
        }
        else
        {
            var temp = Directory.GetParent(DirectoryPath)?.FullName;
            if (Directory.Exists(temp))
            {
                suggestions = Directory.GetDirectories(temp);
            }
            
        }

        return new AutoComplete<string>
               {
                   value = DirectoryPath,

                   suggestions    = suggestions ?? Enumerable.Empty<string>(),
                   completeMethod = _ => Query = _.query,
                   onChange       = OnChange,
                   inputStyle     = { WidthMaximized }
               }
               + FlexGrow(1);
    }

    void OnChange(AutoCompleteChangeParams<string> e)
    { 
        DirectoryPath = e.value;

        if (!Directory.Exists(DirectoryPath))
        {
            return;
        }


        DispatchEvent(() => SelectionChanged, DirectoryPath);
    }
}