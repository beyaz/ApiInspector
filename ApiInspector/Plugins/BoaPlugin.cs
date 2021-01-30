using ApiInspector.MainWindow;
using ApiInspector.Serialization;
using BOA.Common.Helpers;
using BOA.Common.Types;
using Mono.Cecil;
using Newtonsoft.Json.Converters;

namespace ApiInspector.Plugins
{
    public static class BoaPlugin
    {
        public static void Attach()
        {
            _.AddJsonConverter(new MethodDefinitionConverter());
            _.AddJsonConverter(new ObjectHelperConverter());
            _.AddJsonConverter(new DecimalConverter());
            _.AddJsonConverter(new StringEnumConverter());

            _.AddToInvokedMethodReturnValuePipe(NormalizeInvokedMethodReturnValue);
            _.AddToIsVoidMethodPipe(IsVoidMethod);
            _.AddToTypeReferencePipe(GeTypeReference);
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

        static TypeReference GeTypeReference(TypeReference typeReference)
        {
            if (typeReference.FullName.StartsWith("BOA.Common.Types.GenericResponse`1<"))
            {
                typeReference = ((GenericInstanceType) typeReference).GenericArguments[0];
            }

            return typeReference;
        }
    }
}