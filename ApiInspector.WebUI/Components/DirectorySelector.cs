﻿using System.IO;
using ReactWithDotNet.Libraries.PrimeReact;

namespace ApiInspector.WebUI.Components;

public class DirectorySelector : ReactComponent
{
    public string DirectoryPath { get; set; }

    public string Query { get; set; }

    [ReactCustomEvent]
    public Action<string> SelectionChanged { get; set; }

    protected override Element render()
    {
        var suggestions = new List<string>(HistoryOfSearchDirectories.Value);

        if (Directory.Exists(DirectoryPath))
        {
            suggestions.AddRange(Directory.GetDirectories(DirectoryPath));
        }
        else
        {
            var temp = Directory.GetParent(DirectoryPath)?.FullName;
            if (Directory.Exists(temp))
            {
                suggestions.AddRange(Directory.GetDirectories(temp));
            }
        }

        suggestions = suggestions.Distinct().ToList();

        return new AutoComplete<string>
               {
                   value = DirectoryPath,

                   suggestions    = suggestions,
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