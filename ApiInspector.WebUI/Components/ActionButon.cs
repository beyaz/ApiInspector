namespace ApiInspector.WebUI.Components
{
    public class ActionButton : ReactComponent
    {
        public string Label { get; set; }

        public bool IsProcessing { get; set; }

        public string SvgFileName { get; set; }

        protected override Element render()
        {
            return new FlexRowCentered
            {
                children =
                {
                    When(IsProcessing,new LoadingIcon{ wh(17)}),
                    When(!IsProcessing && SvgFileName.HasValue(),new img { Src(GetSvgUrl(SvgFileName)), wh(14), mt(5) }),
                    When(!IsProcessing,new div(Label))
                },
                onClick = ActionButtonOnClick,
                style =
                {
                    Color(BluePrimary),
                    Border($"1px solid {BluePrimary}"),
                    Background("transparent"),
                    BorderRadius(5),
                    Padding(10, 30),
                    CursorPointer
                }
            };
        }

        [ReactCustomEvent]
        public Action OnClick { get; set; }


        void ActionButtonOnClick(MouseEvent _)
        {
            DispatchEvent(() => OnClick);
        }
    }
}
