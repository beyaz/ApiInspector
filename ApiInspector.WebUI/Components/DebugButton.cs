﻿namespace ApiInspector.WebUI.Components;

class DebugButton : ReactComponent
{
    public Action Click { get; set; }

    public bool IsMouseEnter { get; set; }

    public bool IsProcessing { get; set; }

    protected override Element render()
    {
        return new ActionButton
        {
            Label        = "Debug",
            SvgFileName  = "bug",
            OnClick      = Click,
            IsProcessing = IsProcessing,
            Title        = "Attach to 'ApiInspector' process by visual studio or any other ide."
        };
    }
}