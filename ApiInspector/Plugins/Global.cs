using System;
using System.Collections.Generic;
using System.Linq;
using ApiInspector.DataAccess;
using ApiInspector.Serialization;
using BOA.Base;
using Mono.Cecil;

namespace ApiInspector.Plugins
{
    public static class Global
    {
        #region Static Fields
        static readonly List<Func<string, Type, CustomDeserializeResult>> CustomDeserializeFuncList = new List<Func<string, Type, CustomDeserializeResult>>
        {
            BOAPlugin.CustomDeserialize
        };

        static readonly List<Func<object, CustomSerializeResult>> CustomSerializeFuncList = new List<Func<object, CustomSerializeResult>>
        {
            BOAPlugin.CustomSerialize
        };
        #endregion

        #region Public Methods
        public static CustomDeserializeResult CustomDeserialize(string json, Type targetType)
        {
            foreach (var func in CustomDeserializeFuncList)
            {
                var result = func(json, targetType);
                if (result.IsProcessed)
                {
                    return result;
                }
            }

            return new CustomDeserializeResult();
        }

        public static CustomSerializeResult CustomSerialize(object instance)
        {
            foreach (var func in CustomSerializeFuncList)
            {
                var result = func(instance);
                if (result.IsProcessed)
                {
                    return result;
                }
            }

            return new CustomSerializeResult();
        }
        #endregion
    }

    [Serializable]
    public sealed class CustomSerializeResult
    {
        #region Public Properties
        public bool IsProcessed { get; set; }
        public string Json { get; set; }
        #endregion
    }

    public sealed class CustomDeserializeResult
    {
        #region Public Properties
        public object Instance { get; set; }
        public bool IsProcessed { get; set; }
        #endregion
    }

    public sealed class CecilMethodDefinitionSerializationInfo
    {
        #region Public Properties
        public string AssemblyPath { get; set; }
        public string ClassName { get; set; }
        public string MethodNameWithSignature { get; set; }
        #endregion
    }

    static class BOAPlugin
    {
        #region Public Methods
        public static CustomDeserializeResult CustomDeserialize(string json, Type targetType)
        {
            if (targetType == typeof(MethodDefinition))
            {
                return new CustomDeserializeResult
                {
                    IsProcessed = true,

                    Instance = ToMethodDefinition(Serializer.Deserialize<CecilMethodDefinitionSerializationInfo>(json))
                };
            }

            return new CustomDeserializeResult();
        }

        public static CustomSerializeResult CustomSerialize(object instance)
        {
            if (instance is ObjectHelper)
            {
                return new CustomSerializeResult {IsProcessed = true};
            }

            if (instance is MethodDefinition methodDefinition)
            {
                return new CustomSerializeResult
                {
                    IsProcessed = true,
                    Json        = methodDefinition.FullName
                };
            }

            return new CustomSerializeResult();
        }
        #endregion

        #region Methods
        static MethodDefinition ToMethodDefinition(CecilMethodDefinitionSerializationInfo data)
        {
            foreach (var moduleDefinition in AssemblyDefinition.ReadAssembly(data.AssemblyPath).Modules)
            {
                foreach (var item in moduleDefinition.Types)
                {
                    if (item.FullName == data.ClassName)
                    {
                        return item.Methods.FirstOrDefault(m => m.GetMethodNameWithSignature() == data.MethodNameWithSignature);
                    }
                }
            }

            throw new Exception(data.MethodNameWithSignature);
        }
        #endregion
    }
}