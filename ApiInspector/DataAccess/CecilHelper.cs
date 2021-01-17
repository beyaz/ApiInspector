using System.Collections.Generic;
using Mono.Cecil;

namespace ApiInspector.DataAccess
{
    public class CecilHelper
    {
        public static void CollectPropertiesThatCanBeSQLParameter(TypeDefinition typeDefinition, string parentPath,  List<string> items)
        {
            if (typeDefinition == null)
            {
                return;
            }

            foreach (var propertyDefinition in typeDefinition.Properties)
            {
                var isStringType = propertyDefinition.PropertyType.FullName == typeof(string).FullName;

                if (propertyDefinition.PropertyType.IsPrimitive || isStringType)
                {
                    items.Add( parentPath+ propertyDefinition.Name);
                    continue;
                }

                if (!propertyDefinition.PropertyType.IsValueType)
                {
                    CollectPropertiesThatCanBeSQLParameter(propertyDefinition.PropertyType.Resolve(),parentPath +  propertyDefinition.Name+".",items);
                }
            }
        }

        public static IReadOnlyList<string> GetPropertyPathsThatCanBeSQLParameter(object instance)
        {
            var items = new List<string>();

            TypeDefinition typeDefinition = null;
            foreach (var moduleDefinition in AssemblyDefinition.ReadAssembly(instance.GetType().Assembly.Location).Modules)
            {
                foreach (var type in moduleDefinition.Types)
                {
                    if (type.FullName ==instance.GetType().FullName)
                    {
                        typeDefinition = type;
                        break;
                    }
                }
            }

            CollectPropertiesThatCanBeSQLParameter(typeDefinition, "", items);

            return items;
        }

        //public static IReadOnlyList<string> GetSuggestionsFromMethod(MethodDefinition methodDefinition,List<string> names)
        //{


        //    var typeDefinition = methodDefinition.ReturnType.Resolve();
        //    foreach (var propertyDefinition in typeDefinition.Properties)
        //    {
        //        propertyDefinition.Name
        //    }
        //}
    }
}