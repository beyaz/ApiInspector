using System.IO;

namespace ApiInspector.WebUI.Components;

public class DirectorySelector : Component
{
    public string DirectoryPath { get; set; }

    [ReactCustomEvent]
    public Func<string,Task> SelectionChanged { get; set; }

    protected override Element render()
    {
        return new input
        {
            type                     = "text",
            valueBind                = () => DirectoryPath,
            valueBindDebounceTimeout = 700,
            valueBindDebounceHandler = OnFilterTextKeypressCompleted,
            placeholder = "Sample: d:\\work\\",
            style = {ComponentBoxShadow, FontSize12, Padding(8), Border(Solid(1,"#ced4da")), Focus(OutlineNone), BorderRadius(3), Color("#495057") }
        };
    }

    Task OnFilterTextKeypressCompleted()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return Task.CompletedTask;
        }

        DispatchEvent(() => SelectionChanged, DirectoryPath);
        
        return Task.CompletedTask;
    }
}