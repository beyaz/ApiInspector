using System.Collections.Generic;
using ApiInspector.Components;
using ApiInspector.DataAccess;
using ApiInspector.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Test
{
    class AnyClass
    {
        #region Public Methods
        public string AnyMethod_0()
        {
            return "0";
        }

        public string AnyMethod_1(string a, int b, string c)
        {
            return a + "-" + b + "-" + c;
        }

        public static string AnyMethod_2( string a, int b, string c,int d)
        {
            return a + "-" + b + "-" + c+"-"+d;
        }
        #endregion
    }

    [TestClass]
    public class InvocationTests
    {
        #region Public Methods
        [TestMethod]
        public void Should_invoke_non_static__method()
        {
            var invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = "ApiInspector.Test.AnyClass",
                MethodName   = "AnyMethod_0"
            };

            Invoke_method(invocationInfo, "0");


            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = "ApiInspector.Test.AnyClass",
                MethodName   = "AnyMethod_1",
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo()
                    {
                        Type = typeof(string),
                        ValueAsJson = "\"a\""
                    },
                    new InvocationMethodParameterInfo()
                    {
                        Type        = typeof(int),
                        ValueAsJson = "5"
                    },
                    new InvocationMethodParameterInfo()
                    {
                        Type        = typeof(string),
                        ValueAsJson = "\"c\""
                    },

                }
            };

            Invoke_method(invocationInfo, "a-5-c");



            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = "ApiInspector.Test.AnyClass",
                MethodName   = "AnyMethod_2",
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo()
                    {
                        Type        = typeof(string),
                        ValueAsJson = "\"a\""
                    },
                    new InvocationMethodParameterInfo()
                    {
                        Type        = typeof(int),
                        ValueAsJson = "5"
                    },
                    new InvocationMethodParameterInfo()
                    {
                        Type        = typeof(string),
                        ValueAsJson = "\"c\""
                    },
                    new InvocationMethodParameterInfo()
                    {
                        Type        = typeof(int),
                        ValueAsJson = "6"
                    }
                }
            };

            Invoke_method(invocationInfo, "a-5-c-6");
        }
        #endregion

        #region Methods
        void Invoke_method(InvocationInfo invocationInfo, string expectedResponse)
        {
            var builder = new InvocationInfoEditorContextBuilder();

            var context = builder.Build();
            context.Update(DataKeys.AssemblySearchDirectory, string.Empty);

            context.Update(DataKeys.InvocationInfo, invocationInfo);

            Invoker.Execute(context);

            var response = context.Get(DataKeys.ExecutionResponse);

            response.Should().Be(expectedResponse);
        }
        #endregion
    }
}