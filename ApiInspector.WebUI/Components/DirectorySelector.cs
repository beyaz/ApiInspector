using ReactWithDotNet.ThirdPartyLibraries.ReactSuite;

namespace ApiInspector.WebUI.Components;

public class DirectorySelector : Component
{
    public string DirectoryPath { get; set; }

    [CustomEvent]
    public Func<string,Task> SelectionChanged { get; init; }

    protected override Element render()
    {
        List<string> suggestions = [];

        if (DirectoryPath.HasValue())
        {
            var parentDirectory = Directory.GetParent(DirectoryPath);
            if (parentDirectory is not null && parentDirectory.Exists)
            {
                var filterDirectoryName = Path.GetFileNameWithoutExtension(DirectoryPath);

                suggestions =
                [
                    .. from d in parentDirectory.GetDirectories()
                       where d.Name.Contains(filterDirectoryName, StringComparison.OrdinalIgnoreCase)
                       select  Path.Combine(parentDirectory.FullName,  d.Name)
                ];

                suggestions = [.. suggestions.Take(7)];

            }
        }
        
        var autoComplete = new AutoComplete<string>
        {
            value    = DirectoryPath,
            data     = suggestions,
            onChange = OnChange,
            style    = { BorderRadius(5), ComponentBoxShadow },
            placeholder = "Sample: d:\\work\\",
            renderMenuItem = x => new FlexRow(AlignItemsCenter, Gap(4))
            {
                new div
                {
                    Size(24, 16),
                    BorderRadius(4),
                    Border(1, solid, rgb(164, 179, 200)),
                    BackgroundImage(linear_gradient(-45, rgb(203, 212, 226), rgb(164, 179, 200)))
                },
                new span
                {
                    x
                }
            }
        };

        return autoComplete;
    }

    Task OnChange(string selectedValue)
    {
        DirectoryPath = selectedValue;
        
        return OnFilterTextKeypressCompleted();
    }
    
    Task OnFilterTextKeypressCompleted()
    {
        if (!Directory.Exists(DirectoryPath))
        {
            return Task.CompletedTask;
        }
        
        DispatchEvent(SelectionChanged, [DirectoryPath]);
        
        return Task.CompletedTask;
    }
}