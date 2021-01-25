using ApiInspector.MainWindow;

namespace ApiInspector.History
{
    static class DataKeys
    {
        public static DataKey<string> SearchTextKey => Mixin.CreateKey<string>(typeof(DataKeys));
    }
}