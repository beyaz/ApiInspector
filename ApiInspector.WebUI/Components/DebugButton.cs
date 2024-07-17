namespace ApiInspector.WebUI.Components;

sealed class DebugButton : Component
{
    public MouseEventHandler Click { get; init; }

    public bool IsProcessing { get; init; }

    public bool ShowStatusAsFail { get; init; }

    public bool ShowStatusAsSuccess { get; init; }

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
            OnClicked      = Click,
            IsProcessing = IsProcessing,
            TooltipText  = "Press Debug button and attach to 'ApiInspector.exe' process by visual studio or any other ide."
        };
    }
}