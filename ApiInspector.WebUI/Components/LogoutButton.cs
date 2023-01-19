using System.Threading;

namespace ApiInspector.WebUI.Components;

class LogoutButton : ReactComponent
{
    protected override Element render()
    {
        const string svgFileName = "logout";

        return new img
        {
            Src(GetSvgUrl(svgFileName)),
            wh(25),
            OnClick(OnClicked),
            new Style
            {
                hover =
                {
                    BoxShadow("0px 1px 20px 11px rgb(69 42 124 / 15%)"),
                }
            }
        };
    }

    static void ExitAfterThreeSeconds()
    {
        new Thread(() =>
        {
            Thread.Sleep(3000);
            Environment.Exit(1);
        }).Start();
    }

    void OnClicked(MouseEvent _)
    {
        Client.CallJsFunction("CloseWindow");

        ExitAfterThreeSeconds();
    }
}