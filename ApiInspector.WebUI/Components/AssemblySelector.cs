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

            return new AutoComplete
            {
                value    = AssemblyFileName,
                data     = suggestions,
                onChange = OnChange,
                style    = { BorderRadius(5), ComponentBoxShadow }
            };

            Task OnChange(string selectedValue)
            {
                AssemblyFileName = selectedValue;

                if (AssemblyDirectoryPath is not null &&
                    File.Exists(Path.Combine(AssemblyDirectoryPath, AssemblyFileName)))
                {
                    cmp.DispatchEvent<string>(SelectionChanged, AssemblyFileName);
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