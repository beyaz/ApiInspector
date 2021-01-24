using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ApiInspector.InvocationInfoEditor;
using BOA.Base;
using Mono.Cecil;
using static ApiInspector.Plugins.Global;

namespace ApiInspector.DataAccess
{

    
    

    static class CecilHelper
    {

       


        public static string GetMethodNameWithSignature(this MethodDefinition methodDefinition)
        {
            var sb = new StringBuilder(methodDefinition.Name);
            Type.GetType("Mono.Cecil.Mixin,Mono.Cecil", true).GetMethod("MethodSignatureFullName")?.Invoke(null,new object[]{methodDefinition, sb});
            return sb.ToString();
        }

        public static string GetMethodNameWithSignature(this MethodInfo methodInfo)
        {
            var sb = new StringBuilder(methodInfo.Name);
            sb.Append("(");
            sb.Append(string.Join(",", methodInfo.GetParameters().Select(p => p.ParameterType.FullName)));
            sb.Append(")");

            return sb.ToString();
        }

        public static Type GetDotNetType(this TypeReference type)
        {
            return TypeFinder.FindType(type.GetReflectionName());
        }

        static string GetReflectionName(this TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType)type;
                return $"{genericInstance.Namespace}.{type.Name}[{String.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray())}]";
            }
            return type.FullName;
        }

        #region Static Fields
        static readonly List<string> PrimitiveTypes = new List<string>
        {
            typeof(string).FullName,

            typeof(sbyte).FullName,
            typeof(byte).FullName,
            typeof(short).FullName,
            typeof(int).FullName,
            typeof(long).FullName,
            typeof(decimal).FullName,
            typeof(DateTime).FullName,
            typeof(bool).FullName,
            typeof(TimeSpan).FullName,

            FullNameOfNullableSbyte,
            FullNameOfNullableByte,
            FullNameOfNullableShort,
            FullNameOfNullableInt,
            FullNameOfNullableLong,
            FullNameOfNullableDecimal,
            FullNameOfNullableDateTime,
            FullNameOfNullableBoolean,
            FullNameOfNullableTimeSpan
        };
        #endregion

        #region Properties
        static string FullNameOfNullableBoolean => "System.Nullable`1<" + typeof(bool).FullName + ">";
        static string FullNameOfNullableByte => "System.Nullable`1<" + typeof(byte).FullName + ">";
        static string FullNameOfNullableDateTime => "System.Nullable`1<" + typeof(DateTime).FullName + ">";

        static string FullNameOfNullableDecimal => "System.Nullable`1<" + typeof(decimal).FullName + ">";
        static string FullNameOfNullableInt => "System.Nullable`1<" + typeof(int).FullName + ">";
        static string FullNameOfNullableLong => "System.Nullable`1<" + typeof(long).FullName + ">";
        static string FullNameOfNullableSbyte => "System.Nullable`1<" + typeof(sbyte).FullName + ">";
        static string FullNameOfNullableShort => "System.Nullable`1<" + typeof(short).FullName + ">";
        static string FullNameOfNullableTimeSpan => "System.Nullable`1<" + typeof(TimeSpan).FullName + ">";
        #endregion

        #region Public Methods
        public static void CollectPropertiesThatCanBeSQLParameter(TypeDefinition typeDefinition, string parentPath, List<string> items,List<TypeDefinition> history)
        {
            if (typeDefinition == null)
            {
                return;
            }

            if (PrimitiveTypes.Contains(typeDefinition.FullName))
            {
                items.Add(parentPath.RemoveFromEnd("."));
                return;
            }

            if (history.Contains(typeDefinition))
            {
                return;
            }
            history.Add(typeDefinition);

            if (IsCollection( typeDefinition))
            {
                if (parentPath == OutputPrefix+".")
                {
                    items.Add(OutputPrefix);
                    return;
                }

                return;
            }

          

            foreach (var propertyDefinition in typeDefinition.Properties)
            {
                if (propertyDefinition.GetMethod == null || propertyDefinition.SetMethod == null)
                {
                    continue;
                }

                var propertyType = propertyDefinition.PropertyType;

                if (IsCollection(propertyType))
                {
                    continue;
                }

                if (PrimitiveTypes.Contains(propertyDefinition.PropertyType.FullName))
                {
                    items.Add(parentPath + propertyDefinition.Name);
                    continue;
                }

                if (!propertyType.IsValueType)
                {
                    CollectPropertiesThatCanBeSQLParameter(propertyType.Resolve(), parentPath + propertyDefinition.Name + ".", items,history);
                }
            }
        }

       
        public static IReadOnlyList<string> GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(MethodDefinition methodDefinition)
        {
            var roots = new Dictionary<string, TypeDefinition>();
            {
                if (!IsVoidMethod(methodDefinition))
                {
                    roots.Add(OutputPrefix+".", GetReturnTypeDefinitionOf(methodDefinition));
                }

                foreach (var parameterDefinition in methodDefinition.Parameters)
                {
                    if (parameterDefinition.ParameterType.FullName == typeof(ObjectHelper).FullName)
                    {
                        continue;
                    }

                    roots.Add(PrefixCharacter + parameterDefinition.Name + ".", parameterDefinition.ParameterType.Resolve());
                }
            }

            var items = new List<string>();
            {
                foreach (var pair in roots)
                {
                    CollectPropertiesThatCanBeSQLParameter(pair.Value, pair.Key, items,history:new List<TypeDefinition>());
                }
            }
            
            return items;
        }

        public const string PrefixCharacter = "@";
        public const string OutputPrefix= PrefixCharacter+ "output";

        public static IReadOnlyList<string> GetPropertyPathsThatCanBeSQLParameter(object instance)
        {
            var items   = new List<string>();

            var history = new List<TypeDefinition>();

            CollectPropertiesThatCanBeSQLParameter(GeTypeDefinitionFromType(instance.GetType()), string.Empty, items,history);

            return items;
        }

        public static bool IsCollection(TypeReference typeReference)
        {
            if (typeReference == null)
            {
                return false;
            }

            if (typeReference.FullName.StartsWith("System.Collections.Generic.List`1"))
            {
                return true;
            }

            if (typeReference.FullName.StartsWith("System.Collections.Generic.IReadOnlyList`1"))
            {
                return true;
            }

            if (typeReference.FullName.StartsWith("System.Collections.Generic.ICollection`1"))
            {
                return true;
            }

            if (typeReference.FullName.StartsWith("System.Collections.Generic.IReadOnlyCollection`1"))
            {
                return true;
            }

            if (typeReference.FullName.StartsWith("System.Array"))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Methods
        static TypeDefinition GeTypeDefinitionFromType(Type type)
        {
            foreach (var moduleDefinition in AssemblyDefinition.ReadAssembly(type.Assembly.Location).Modules)
            {
                foreach (var item in moduleDefinition.Types)
                {
                    if (item.FullName == type.FullName)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}