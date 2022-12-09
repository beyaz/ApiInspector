namespace ApiInspector.WebUI;

class MainWindowModel
{
        
}
class MainWindow: ReactComponent<MainWindowModel>
{
    protected override Element render()
    {
        var borderColor = "#d5d5d8";
        
        return new FlexRow(Padding(10), PositionAbsolute, Top(0),Bottom(0),Left(0),Right(0), Background("#eff3f8"))
        {
            new FlexColumn(Border($"1px solid {borderColor}"), WidthHeightMaximized, Background("white"))
            {
                   new FlexRow(PaddingLeftRight(16), PaddingTopBottom(8), BorderBottom($"1px solid {borderColor}"))
                   {
                       new FlexRow(Gap(5))
                       {
                           AlignItemsCenter,
                           new h3{"Api Inspector"}, new h5{ " (.net method invoker) " }
                       }
                   } ,
                   
                   new FlexRow(HeightMaximized)
                   {
                       new FlexColumn(Width(300), HeightMaximized)
                       {
                           "LeftMenu",
                           BorderRight($"1px solid {borderColor}")
                       },
                       
                       new FlexRowCentered
                       {
                           "Aloha",
                           FlexGrow(1)
                       }
                       
                   }
            }
        };
    }
}