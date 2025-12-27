namespace ApiInspector.WebUI.Components;

sealed class ExecuteButton : PureComponent
{
    public MouseEventHandler Click { get; init; }
    
    public required ActionButtonStatus Status { get; init; }

    protected override Element render()
    {
        var svgFileName = "play";

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
            Status       = Status,
            Label        = "Execute",
            SvgFileName  = svgFileName,
            OnClicked    = Click,
            TooltipText  = "Executes selected method by given parameters above then show results in below."
        };
    }
}