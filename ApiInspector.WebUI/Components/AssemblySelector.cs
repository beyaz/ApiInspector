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
                suggestions = Directory.GetFiles(AssemblyDirectoryPath).Where(ExeOrDllFile).Where(x => x.Contains(AssemblyFileName ?? string.Empty, StringComparison.OrdinalIgnoreCase)).Select(Path.GetFileName).Take(7);
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
                    cmp.DispatchEvent(SelectionChanged, AssemblyFileName);
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