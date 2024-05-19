using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;

namespace ApiInspector.WebUI.Components;

sealed class ActionButton : Component
{
    public bool IsProcessing { get; set; }

    public string Label { get; init; }

    [CustomEvent]
    public Func<Task> OnClicked { get; init; }

    public string SvgFileName { get; init; }

    public string TooltipText { get; init; }

    protected override Element render()
    {
        var loadingIcon = IsProcessing is false ? null : new LoadingIcon { Size(20), MarginRight(10) };

        var icon = !IsProcessing && SvgFileName.HasValue() ? new img { Src(GetSvgUrl(SvgFileName)), Size(20), MarginRight(5) } : null;

        var buttonStyle = new Style
        {
            Color(BluePrimary),
            Border(1, solid, BluePrimary),
            Background(transparent),
            BorderRadius(5),
            Padding(10, 20),
            CursorPointer
        };

        var onClick = IsProcessing ? null : OnClick(ActionButtonOnClick);

        var content = new FlexRowCentered(buttonStyle, onClick)
        {
            loadingIcon,
            icon,
            new div(Label)
        };

        return ArrangeTooltip(content);
    }

    Task ActionButtonOnClick(MouseEvent _)
    {
        IsProcessing = true;

        DispatchEvent(OnClicked);

        return Task.CompletedTask;
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