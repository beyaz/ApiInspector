namespace ApiInspector.Invoking
{
    static class __
    {
        public static bool IsSuccess(InvokeOutput invokeOutput)
        {
            return invokeOutput.Error == null;
        }

        public static readonly InvokeOutput EODSuccess = new InvokeOutput();
    }
}