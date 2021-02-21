using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BOA.Base;
using Mono.Cecil;
using static ApiInspector._;

namespace ApiInspector.DataAccess
{
    static class CecilHelper
    {
        #region Constants
        public const string OutputPrefix = IntellisensePrefix + "output";
        #endregion

        #region Static Fields
        static readonly Func<string, bool> IsPrimitiveType;
        #endregion

        #region Constructors
        static CecilHelper()
        {
            // IsPrimitiveType
            {
                var primitiveTypes = new List<string>
                {
                    typeof(string).FullName
                };

                var types = new[]
                {
                    typeof(sbyte),
                    typeof(byte),
                    typeof(short),
                    typeof(int),
                    typeof(long),
                    typeof(decimal),
                    typeof(DateTime),
                    typeof(bool),
                    typeof(TimeSpan)
                };

                foreach (var type in types)
                {
                    primitiveTypes.Add(type.FullName);
                    primitiveTypes.Add("System.Nullable`1<" + type.FullName + ">");
                }

                IsPrimitiveType = fullTypeName => primitiveTypes.Contains(fullTypeName);
            }
        }
        #endregion

        #region Public Methods
        public static void CollectPropertiesThatCanBeSQLParameter(TypeDefinition typeDefinition, string parentPath, List<string> items, List<TypeDefinition> history)
        {
            if (typeDefinition == null)
            {
                return;
            }

            if (IsPrimitiveType(typeDefinition.FullName))
            {
                items.Add(parentPath.RemoveFromEnd("."));
                return;
            }

            if (history.Contains(typeDefinition))
            {
                return;
            }

            history.Add(typeDefinition);

            if (IsCollection(typeDefinition))
            {
                if (parentPath == OutputPrefix + ".")
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

                if (IsPrimitiveType(propertyDefinition.PropertyType.FullName))
                {
                    items.Add(parentPath + propertyDefinition.Name);
                    continue;
                }

                if (!propertyType.IsValueType)
                {
                    CollectPropertiesThatCanBeSQLParameter(propertyType.Resolve(), parentPath + propertyDefinition.Name + ".", items, history);
                }
            }
        }

        public static Type GetDotNetType(this TypeReference type)
        {
            return FindTypeByFullName(type.GetReflectionName());
        }

        public static string GetMethodNameWithSignature(this MethodDefinition methodDefinition)
        {
            var sb = new StringBuilder(methodDefinition.Name);
            Type.GetType("Mono.Cecil.Mixin,Mono.Cecil", true).GetMethod("MethodSignatureFullName")?.Invoke(null, new object[] {methodDefinition, sb});
            return sb.ToString();
        }

        public static string GetMethodNameWithSignature(this MethodInfo methodInfo)
        {
            var sb = new StringBuilder(methodInfo.Name);
            sb.Append("(");
            sb.Append(string.Join(",", methodInfo.GetParameters().Select(p => GetFullName(p.ParameterType))));
            sb.Append(")");

            return sb.ToString();
        }

        public static IReadOnlyList<string> GetPropertyPathsThatCanBeSQLParameter(object instance)
        {
            var items = new List<string>();

            var history = new List<TypeDefinition>();

            CollectPropertiesThatCanBeSQLParameter(GeTypeDefinitionFromType(instance.GetType()), string.Empty, items, history);

            return items;
        }

        public static IReadOnlyList<string> GetPropertyPathsThatCanBeSQLParameterFromMethodDefinition(MethodDefinition methodDefinition)
        {
            var roots = new Dictionary<string, TypeDefinition>();
            {
                if (!IsVoidMethod(methodDefinition))
                {
                    roots.Add(OutputPrefix + ".", GetTypeReference(methodDefinition.ReturnType).Resolve());
                }

                foreach (var parameterDefinition in methodDefinition.Parameters)
                {
                    if (parameterDefinition.ParameterType.FullName == typeof(ObjectHelper).FullName)
                    {
                        continue;
                    }

                    roots.Add(IntellisensePrefix + parameterDefinition.Name + ".", parameterDefinition.ParameterType.Resolve());
                }
            }

            var items = new List<string>();
            {
                foreach (var pair in roots)
                {
                    CollectPropertiesThatCanBeSQLParameter(pair.Value, pair.Key, items, history: new List<TypeDefinition>());
                }
            }

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
        static string GetReflectionName(this TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var genericInstance = (GenericInstanceType) type;
                return $"{genericInstance.Namespace}.{type.Name}[{string.Join(",", genericInstance.GenericArguments.Select(p => p.GetReflectionName()).ToArray())}]";
            }

            return type.FullName;
        }

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