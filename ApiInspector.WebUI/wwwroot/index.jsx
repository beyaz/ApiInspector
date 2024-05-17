// import core library
import ReactWithDotNet from "./react-with-dotnet/react-with-dotnet";

// you can comment these imports according to your project dependency

import "./react-with-dotnet/libraries/rsuite/all";
import "./react-with-dotnet/libraries/react-free-scrollbar/all";
import "./react-with-dotnet/libraries/MonacoEditorReact/all";
import "./react-with-dotnet/libraries/mui-core/all";
import "./react-with-dotnet/libraries/react-split/all";

ReactWithDotNet.RegisterExternalJsObject("CloseWindow", () => window.close());

export { ReactWithDotNet };

