using ReactWithDotNet.ThirdPartyLibraries.MUI.Material;

namespace ApiInspector.WebUI.Components;

public class ActionButton : Component
{
    public bool IsProcessing { get; set; }

    public string Label { get; init; }

    [CustomEvent]
    public Func<Task> OnClick { get; init; }

    public string SvgFileName { get; init; }

    public string TooltipText { get; init; }

    protected override Element render()
    {
        return ArrangeTooltip(new FlexRowCentered
        {
            children =
            {
                IsProcessing is false ? null : new LoadingIcon { Size(20), MarginRight(10) },
                !IsProcessing && SvgFileName.HasValue() ? new img { Src(GetSvgUrl(SvgFileName)), Size(20), MarginRight(5) } : null,
                new div(Label)
            },
            onClick = ActionButtonOnClick,
            style =
            {
                Color(BluePrimary),
                Border(1, solid, BluePrimary),
                Background(transparent),
                BorderRadius(5),
                Padding(10, 20),
                CursorPointer
            }
        });
    }

    Task ActionButtonOnClick(MouseEvent _)
    {
        if (IsProcessing)
        {
            throw new InvalidOperationException("Action button already processing...");
        }

        IsProcessing = true;

        DispatchEvent(OnClick);

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