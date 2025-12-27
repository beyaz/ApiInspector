using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;

namespace ApiInspector.WebUI.Components;

sealed class ActionButton : PureComponent
{
    public bool IsProcessing { get; init; }

    public string Label { get; init; }

    public required MouseEventHandler OnClicked { get; init; }

    public string SvgFileName { get; init; }

    public string TooltipText { get; init; }

    

    protected override Element render()
    {
        var isProcessing = IsProcessing;

        var loadingIcon = isProcessing is false ? null : new LoadingIcon { Size(20), MarginRight(10) };

        var icon = !isProcessing && SvgFileName.HasValue() ? new img { Src(GetSvgUrl(SvgFileName)), Size(20), MarginRight(5) } : null;

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
            loadingIcon,
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