﻿using System.IO;

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
            valueBindDebounceHandler = OnFilterTextKeypressCompleted
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