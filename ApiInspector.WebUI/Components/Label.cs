namespace ApiInspector.WebUI.Components;

class Label : ReactComponent
{
    public string Text { get; set; }

    protected override Element render()
    {
        return new div { FontSizeSmall, FontWeight600, Text };
    }
}