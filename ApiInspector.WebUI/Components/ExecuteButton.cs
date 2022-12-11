﻿namespace ApiInspector.WebUI.Components;

class ExecuteButton : ReactComponent
{
    public Action Click { get; set; }

    protected override Element render()
    {
        return new ActionButton { Label = "Execute", SvgFileName = "play", OnClick = Click };
    }
}