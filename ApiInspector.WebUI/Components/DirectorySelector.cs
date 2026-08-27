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
        
        foreach (var path in Config.DirectorySuggestions)
        {
            if (path.EndsWith("*",StringComparison.OrdinalIgnoreCase))
            {
                suggestions.AddRange(Directory.GetDirectories(path.TrimEnd('*').Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)));
                continue;
            }
            
            suggestions.Add(path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
        }
        
        
        if (DirectoryPath.HasValue() && Path.IsPathRooted(DirectoryPath))
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
            },
            filterBy= """
                      (function(keyword, option)
                      {
                          var parts = keyword
                              .split(/[\\/:._-]+/)
                              .filter(Boolean)
                              .map(x => x.toLocaleLowerCase());

                          var label = option.label.toLocaleLowerCase();

                          return parts.every(part => label.includes(part));
                      })
                      """
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