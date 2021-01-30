using ApiInspector.MainWindow;
using BOA.Common.Helpers;
using BOA.Common.Types;
using Mono.Cecil;

namespace ApiInspector.Plugins
{
    public static class Global
    {

        public static TypeReference GetReturnTypeReferenceOf(MethodDefinition methodDefinition)
        {
            var returnTypeReference = methodDefinition.ReturnType;

            if (returnTypeReference.FullName.StartsWith("BOA.Common.Types.GenericResponse`1<"))
            {
                returnTypeReference = ((GenericInstanceType) returnTypeReference).GenericArguments[0];
            }

            return returnTypeReference;
        }

        

        
    }
}