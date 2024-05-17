using System.IO;
using ReactWithDotNet.ThirdPartyLibraries.ReactSuite;

namespace ApiInspector.WebUI.Components;

public class AssemblySelector
{
    public static Element CreateAssemblySelectorInput(string AssemblyDirectoryPath, string AssemblyFileName, Func<string, Task> SelectionChanged)
    {
        return FC(cmp =>
        {
            var suggestions = Enumerable.Empty<string>();

            if (Directory.Exists(AssemblyDirectoryPath))
            {

                var assemblyFileName = (AssemblyFileName ?? string.Empty).Replace("\"", string.Empty).Replace("\"", string.Empty);
                
                suggestions = Directory.GetFiles(AssemblyDirectoryPath)
                    .Select(Path.GetFileName)
                    .Where(ExeOrDllFile)
                    .Where(x => x.Contains(assemblyFileName, StringComparison.OrdinalIgnoreCase))
                    .Take(7);
            }

            return new AutoComplete<string>
            {
                value    = AssemblyFileName,
                data     = suggestions,
                onChange = OnChange,
                style    = { BorderRadius(5), ComponentBoxShadow },
                renderMenuItem = x=>new FlexRow(AlignItemsCenter, Gap(4))
                {
                    new div
                    {
                        Size(24,16),
                        BorderRadius(4),
                        Border(1,solid,rgb(164, 179, 200)),
                        BackgroundImage(linear_gradient(-45,rgb(203, 212, 226),rgb(164, 179, 200)))
                    },
                    new span
                    {
                        x
                    }
                    
                    
                }
            };

            Task OnChange(string selectedValue)
            {
                AssemblyFileName = selectedValue;

                if (AssemblyDirectoryPath is not null &&
                    File.Exists(Path.Combine(AssemblyDirectoryPath, AssemblyFileName)))
                {
                    cmp.DispatchEvent(SelectionChanged, [AssemblyFileName]);
                }
                
                return Task.CompletedTask;
            }
        });

        static bool ExeOrDllFile(string x)
        {
            return Path.GetExtension(x) == ".dll" || Path.GetExtension(x) == ".exe";
        }
    }
}