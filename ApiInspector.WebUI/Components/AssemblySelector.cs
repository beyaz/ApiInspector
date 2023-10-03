﻿using System.IO;
using ReactWithDotNet.ThirdPartyLibraries.ReactSuite;

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
            suggestions = Directory.GetFiles(AssemblyDirectoryPath).Where(x => Path.GetExtension(x) == ".dll" || Path.GetExtension(x) == ".exe").Where(x => x.Contains(Query ?? string.Empty, StringComparison.OrdinalIgnoreCase)).Select(Path.GetFileName).Take(7);
        }

        return new AutoComplete
               {
                   value = AssemblyFileName,
                   data           = suggestions,
                   
                   onChange       = OnChange,
                   style     = { WidthMaximized }
               }
               + FlexGrow(1);
    }

    void OnChange(string selectedValue)
    {
        AssemblyFileName = selectedValue;

        if (File.Exists(Path.Combine(AssemblyDirectoryPath, AssemblyFileName)))
        {
            DispatchEvent(() => SelectionChanged, AssemblyFileName);
        }
    }
}