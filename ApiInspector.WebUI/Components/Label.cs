namespace ApiInspector.WebUI.Components;

class Label : ReactPureComponent
{
    public string Text { get; set; }

    protected override Element render()
    {
        return new div { FontSizeSmall, FontWeight600, Text };
    }
}

abstract class ReactPureComponent: PureComponent
{
    
}

abstract class ReactComponent: Component
{
    
    
}

