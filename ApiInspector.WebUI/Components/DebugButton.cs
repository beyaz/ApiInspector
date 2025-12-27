namespace ApiInspector.WebUI.Components;

sealed class DebugButton : Component
{
    public MouseEventHandler Click { get; init; }
    
    public required ActionButtonStatus Status { get; init; }

    protected override Element render()
    {
        var svgFileName = "bug";

        if (Status == ActionButtonStatus.Success)
        {
            svgFileName = "success";
        }

        if (Status == ActionButtonStatus.Fail)
        {
            svgFileName = "fail";
        }

        return new ActionButton
        {
            Status = Status,
            Label        = "Debug",
            SvgFileName  = svgFileName,
            OnClicked      = Click,
            TooltipText  = "Press Debug button and attach to 'ApiInspector.exe' process by visual studio or any other ide."
        };
    }
}