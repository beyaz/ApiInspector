namespace ApiInspector.WebUI.Components;

class DebugButton: ReactComponent
{
    public Action Click { get; set; }
    
    protected override Element render()
    {
        return new ActionButton { Label = "Debug", SvgFileName = "bug", OnClick = Click};
    }
}

class RefreshButton : ReactComponent
{
    public Action Click { get; set; }

    protected override Element render()
    {
        return new ActionButton { Label = "Refresh", SvgFileName = "refresh", OnClick = Click };
    }
}