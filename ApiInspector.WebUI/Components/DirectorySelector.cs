using System.IO;

namespace ApiInspector.WebUI.Components;

public class DirectorySelector : ReactComponent
{
    public string DirectoryPath { get; set; }

    [ReactCustomEvent]
    public Action<string> SelectionChanged { get; set; }

    protected override Element render()
    {
        return new input
        {
            type                     = "text",
            valueBind                = () => DirectoryPath,
            valueBindDebounceTimeout = 700,
            valueBindDebounceHandler = OnFilterTextKeypressCompleted
        };
    }

    void OnFilterTextKeypressCompleted()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return;
        }

        DispatchEvent(() => SelectionChanged, DirectoryPath);
    }
}