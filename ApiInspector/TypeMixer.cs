

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ApiInspector
{
    public static class TypeMixer<T>
    {
        static readonly BindingFlags visibilityFlags = BindingFlags.Public | BindingFlags.Instance;

        public static K ExtendWith<K>(T source = default(T))
        {
            var assemblyName = new Guid().ToString();

            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule("Module");
            var type = module.DefineType(typeof(T).Name+"_"+typeof(K).Name, TypeAttributes.Public, typeof(T));
            var fieldsList = new List<string>();

            type.AddInterfaceImplementation(typeof(K));

            foreach (var v in typeof(K).GetProperties())
            {
                fieldsList.Add(v.Name);

                var field = type.DefineField("_" + v.Name.ToLower(), v.PropertyType, FieldAttributes.Private);
                var property = type.DefineProperty(v.Name, PropertyAttributes.None, v.PropertyType, new Type[0]);
                var getter = type.DefineMethod("get_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, v.PropertyType, new Type[0]);
                var setter = type.DefineMethod("set_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, new Type[] { v.PropertyType });

                var getGenerator = getter.GetILGenerator();
                var setGenerator = setter.GetILGenerator();

                getGenerator.Emit(OpCodes.Ldarg_0);
                getGenerator.Emit(OpCodes.Ldfld, field);
                getGenerator.Emit(OpCodes.Ret);

                setGenerator.Emit(OpCodes.Ldarg_0);
                setGenerator.Emit(OpCodes.Ldarg_1);
                setGenerator.Emit(OpCodes.Stfld, field);
                setGenerator.Emit(OpCodes.Ret);

                property.SetGetMethod(getter);
                property.SetSetMethod(setter);

                type.DefineMethodOverride(getter, v.GetGetMethod());
                type.DefineMethodOverride(setter, v.GetSetMethod());
            }

            if (source != null)
            {
                foreach (var v in source.GetType().GetProperties())
                {
                    if (fieldsList.Contains(v.Name))
                    {
                        continue;
                    }

                    fieldsList.Add(v.Name);

                    var field = type.DefineField("_" + v.Name.ToLower(), v.PropertyType, FieldAttributes.Private);

                    var property = type.DefineProperty(v.Name, PropertyAttributes.None, v.PropertyType, new Type[0]);
                    var getter = type.DefineMethod("get_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, v.PropertyType, new Type[0]);
                    var setter = type.DefineMethod("set_" + v.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual, null, new Type[] { v.PropertyType });

                    var getGenerator = getter.GetILGenerator();
                    var setGenerator = setter.GetILGenerator();

                    getGenerator.Emit(OpCodes.Ldarg_0);
                    getGenerator.Emit(OpCodes.Ldfld, field);
                    getGenerator.Emit(OpCodes.Ret);

                    setGenerator.Emit(OpCodes.Ldarg_0);
                    setGenerator.Emit(OpCodes.Ldarg_1);
                    setGenerator.Emit(OpCodes.Stfld, field);
                    setGenerator.Emit(OpCodes.Ret);

                    property.SetGetMethod(getter);
                    property.SetSetMethod(setter);
                }
            }

            var newObject = (K)Activator.CreateInstance(type.CreateType());

            return source == null ? newObject : CopyValues(source, newObject);
        }

        private static K CopyValues<K>(T source, K destination)
        {
            foreach (PropertyInfo property in source.GetType().GetProperties(visibilityFlags))
            {
                var prop = destination.GetType().GetProperty(property.Name, visibilityFlags);
                if (prop != null && prop.CanWrite)
                    prop.SetValue(destination, property.GetValue(source), null);
            }

            return destination;
        }
    }
}