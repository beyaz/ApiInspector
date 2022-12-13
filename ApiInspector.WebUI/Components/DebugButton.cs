﻿namespace ApiInspector.WebUI.Components;

class DebugButton : ReactComponent
{
    public Action Click { get; set; }

    public bool IsMouseEnter { get; set; }

    public bool IsProcessing { get; set; }

    public bool ShowStatusAsFail { get; set; }

    public bool ShowStatusAsSuccess { get; set; }
    
    protected override Element render()
    {
        var svgFileName = "bug";

        if (ShowStatusAsSuccess)
        {
            svgFileName = "success";
        }

        if (ShowStatusAsFail)
        {
            svgFileName = "fail";
        }

        return new ActionButton
        {
            Label        = "Debug",
            SvgFileName  = svgFileName,
            OnClick      = Click,
            IsProcessing = IsProcessing,
            Title        = "Attach to 'ApiInspector' process by visual studio or any other ide."
        };
    }
}