using ApiInspector.MainWindow;
using BOA.Common.Helpers;
using BOA.Common.Types;
using Mono.Cecil;

namespace ApiInspector.Plugins
{
    public static class Global
    {

        public static object NormalizeInvokedMethodReturnValue(object value)
        {
            if (value ==null)
            {
                return null;
            }

            var type = value.GetType();

            
            if (type.FullName?.StartsWith("BOA.Common.Types.GenericResponse`1") == true)
            {
                var responseBase = (ResponseBase)value;
                if (!responseBase.Success)
                {
                    throw new ExecutionResponseHasErrorException(StringHelper.ResultToDetailedString(responseBase.Results));
                }
                return type.GetProperty("Value")?.GetValue(value);
            }

            return value;
        }

        public static TypeDefinition GetReturnTypeDefinitionOf(MethodDefinition methodDefinition)
        {
            return GetReturnTypeReferenceOf(methodDefinition).Resolve();
        }

        public static TypeReference GetReturnTypeReferenceOf(MethodDefinition methodDefinition)
        {
            var returnTypeReference = methodDefinition.ReturnType;

            if (returnTypeReference.FullName.StartsWith("BOA.Common.Types.GenericResponse`1<"))
            {
                returnTypeReference = ((GenericInstanceType)returnTypeReference).GenericArguments[0];
            }

            return returnTypeReference;
        }

        public static bool IsVoidMethod(MethodDefinition methodDefinition)
        {
            var fullTypeName = methodDefinition.ReturnType.FullName;

            return fullTypeName == "System.Void" || fullTypeName== "BOA.Common.Types.ResponseBase";
        }


        #region Static Fields
       

       
       

       


        #endregion
    }

   

   


    
}