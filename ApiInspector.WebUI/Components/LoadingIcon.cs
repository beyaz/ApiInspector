﻿namespace ApiInspector.WebUI.Components;

// Taken from https://www.w3schools.com/howto/tryit.asp?filename=tryhow_css_loader
public class LoadingIcon : PureComponent
{
    protected override Element render()
    {
        return new div
        {
            new style
            {
                """
                .loader {
                  border: 1px solid #f3f3f3;
                  border-radius: 50%;
                  border-top: 1px solid #3498db;
                  width: 100%;
                  height: 100%;
                  -webkit-animation: spin 1s linear infinite; /* Safari */
                  animation: spin 1s linear infinite;
                }

                /* Safari */
                @-webkit-keyframes spin {
                  0% { -webkit-transform: rotate(0deg); }
                  100% { -webkit-transform: rotate(360deg); }
                }

                @keyframes spin {
                  0% { transform: rotate(0deg); }
                  100% { transform: rotate(360deg); }
                }
                """,

                new CssClass("loader",
                [
                    Border(1, solid, "#f3f3f3"),
                    BorderRadius("50%"),
                    BorderTop("#3498db"),
                    SizeFull,
                    WebkitAnimation("spin 1s linear infinite"),
                    Animation("spin 1s linear infinite")
                ])
            },

            new div { className = "loader", style = { Size("100%") } }
        };
    }
}