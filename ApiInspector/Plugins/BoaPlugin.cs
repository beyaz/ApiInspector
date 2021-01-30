using ApiInspector.MainWindow;
using ApiInspector.Serialization;
using BOA.Common.Helpers;
using BOA.Common.Types;
using Mono.Cecil;

namespace ApiInspector.Plugins
{
    public static class BoaPlugin
    {
        public static void Attach()
        {
            _.AddJsonConverter(new MethodDefinitionConverter());
            _.AddJsonConverter(new ObjectHelperConverter());
            _.AddJsonConverter(new DecimalConverter());
            _.AddJsonConverter(new Newtonsoft.Json.Converters.StringEnumConverter());

            _.AddToInvokedMethodReturnValuePipe(NormalizeInvokedMethodReturnValue);
            _.AddToIsVoidMethodPipe(IsVoidMethod);
        }


        static object NormalizeInvokedMethodReturnValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();

            if (type.FullName?.StartsWith("BOA.Common.Types.GenericResponse`1") == true)
            {
                var responseBase = (ResponseBase) value;
                if (!responseBase.Success)
                {
                    throw new ExecutionResponseHasErrorException(StringHelper.ResultToDetailedString(responseBase.Results));
                }

                return type.GetProperty("Value")?.GetValue(value);
            }

            return value;
        }

         static bool IsVoidMethod(MethodDefinition methodDefinition)
        {
            var fullTypeName = methodDefinition.ReturnType.FullName;

            return fullTypeName == "System.Void" || fullTypeName == "BOA.Common.Types.ResponseBase";
        }
    }
}