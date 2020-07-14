using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ApiInspector.DataAccess;
using BOA.Base;
using BOA.Base.Data;
using BOA.Common.Types;
using BOA.DataFlow;
using BOA.UnitTestHelper;
using Mono.Cecil;

namespace ApiInspector.Components
{
    static class Logic
    {


        public static void Execute(DataContext context)
        {
            var assemblyName = context.Get(DataKeys.AssemblyName);
            var methodDefinition = context.Get(DataKeys.MethodDefinition);

            var targetEnvironment = context.Get(DataKeys.TargetEnvironment);

            var className = context.Get(DataKeys.ClassName);

            ExecutionDataContext executionDataContext;
            if (targetEnvironment.IndexOf("dev",StringComparison.OrdinalIgnoreCase) >=0)
            {
                executionDataContext = new BOATestContextDev().objectHelper.Context;

                executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                {
                    {Databases.BanksoftCC, @"Data Source=srvxdev\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                };
            }
            else  if (targetEnvironment.IndexOf("test",StringComparison.OrdinalIgnoreCase) >=0)
            {
                executionDataContext = new BOATestContextTest().objectHelper.Context;

                //executionDataContext.DBLayer.ConnectionMock = new Dictionary<Databases, string>
                //{
                //    {Databases.BanksoftCC, @"Data Source=srvxtest\zumrut;Initial Catalog=KrediKuveyt;Min Pool Size=10; Max Pool Size=100;Application Name=BOAApp;Integrated Security=true;"}
                //};
            }
            else
            {
                throw new NotImplementedException(nameof(targetEnvironment));
            }

            var targetType = Type.GetType($"{className},{assemblyName}", true);

            var instance = CreateInstance(targetType,executionDataContext);

            var methodInfo = targetType.GetMethod(methodDefinition.Name);
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var parameters = new List<object>();



            var response = methodInfo.Invoke(instance,parameters.ToArray());

            context.Update(DataKeys.ExecutionResponse,response);

        }
        
        static object CreateInstance(Type targetType, ExecutionDataContext executionDataContext)
        {
            // constructor with ExecutionDataContext
            { 
                var constructorInfo = targetType.GetConstructor(new[]
                {
                    typeof(ExecutionDataContext)
                });
                if (constructorInfo != null)
                {
                    var instance = constructorInfo.Invoke(new object[]
                    {
                        executionDataContext
                    });
                    return instance;
                }
            }

            // simple constructor
            {
                var instance =  Activator.CreateInstance(targetType) as ObjectHelper;
                if (instance != null)
                {
                    instance.Context = executionDataContext;    
                }
            
                return instance;
            }
        }


        #region Public Methods
        public static void OnAssemblyNameChanged(DataContext context)
        {
            var assemblySearchDirectory = context.Get(DataKeys.AssemblySearchDirectory);
            var assemblyName            = context.Get(DataKeys.AssemblyName);

            context.Update(DataKeys.AssemblyFilePath, Path.Combine(assemblySearchDirectory, assemblyName));

            ClassNamesInAssembly.Load(context);
        }

        public static void OnMethodNameSelected(DataContext context)
        {
            var className        = context.Get(DataKeys.ClassName);
            var methodName       = context.Get(DataKeys.MethodName);
            var assemblyFilePath = context.Get(DataKeys.AssemblyFilePath);

            var typeDefinition = CecilHelper.FindType(context, assemblyFilePath, className);

            var methodDefinition = typeDefinition.Methods.FirstOrDefault(x => x.Name == methodName);

            if (methodDefinition == null)
            {
                return;
            }

            context.Update(DataKeys.MethodDefinition, methodDefinition);


            UpdateUI(context);


        }

        static void UpdateUI(DataContext context)
        {
            var panel = context.Get(DataKeys.ParametersPanel);

            panel.Children.Clear();
            
            
            var methodDefinition = context.Get(DataKeys.MethodDefinition);

            foreach (var parameterDefinition in methodDefinition.Parameters)
            {
                var item = Create(parameterDefinition);

                panel.Children.Add(item);
            }
        }

        static StackPanel Create(ParameterDefinition definition)
        {
            var sp = new StackPanel();

            var label = new Label
            {
                Content = definition.Name,
                FontWeight = FontWeights.Bold
            };

            var editor = new TextBox();

            if (definition.ParameterType.FullName == typeof(ObjectHelper).FullName)
            {
                editor.IsEnabled = false;
            }

            sp.Children.Add(label);
            sp.Children.Add(editor);

            return sp;
        }

        #endregion
    }
}