using BOA.DataFlow;

namespace ApiInspector.DataAccess
{
    static class DataKeys
    {
        public static  readonly DataKey<string> MethodName = new DataKey<string>(nameof(MethodName));
        public static  readonly DataKey<string> AssemblyName = new DataKey<string>(nameof(AssemblyName));
        public static  readonly DataKey<string> ClassName = new DataKey<string>(nameof(ClassName));
    }
}