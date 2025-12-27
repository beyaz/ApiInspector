using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;

namespace ApiInspector.WebUI.Components;

sealed class ActionButton : PureComponent
{
    public string Label { get; init; }

    public required MouseEventHandler OnClicked { get; init; }

    public string SvgFileName { get; init; }

    public string TooltipText { get; init; }

    public required ActionButtonStatus Status { get; init; }

    protected override Element render()
    {
        var isProcessing = Status == ActionButtonStatus.Executing;

        Element icon = null;
        {
            if (isProcessing)
            {
                icon = new LoadingIcon { Size(20), MarginRight(10) };
            }
            else if(SvgFileName.HasValue())
            {
                icon = new img { Src(GetSvgUrl(SvgFileName)), Size(20), MarginRight(5) };
            }
        }
        
        var buttonStyle = new Style
        {
            Color(BluePrimary),
            Border(1, solid, BluePrimary),
            Background(transparent),
            BorderRadius(5),
            Padding(10, 20),
            CursorPointer
        };

        var content = new FlexRowCentered(buttonStyle, OnClick(OnClicked))
        {
            icon,
            new div{Label}
        };

        return ArrangeTooltip(content);
    }
    
    Element ArrangeTooltip(Element content)
    {
        if (TooltipText.HasValue())
        {
            return new Tooltip
            {
                arrow = true,
                title = TooltipText,
                children =
                {
                    content
                }
            };
        }

        return content;
    }
}