using System.Collections.Generic;
using ApiInspector.Models;
using ApiInspector.TestData;
using BOA.DataFlow;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiInspector.Invoking
{
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
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_0)
            };

            Invoke_method(invocationInfo, "0");

            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_1),
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "\"a\""
                    },
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "5"
                    },
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "\"c\""
                    },
                }
            };

            Invoke_method(invocationInfo, "a-5-c");

            invocationInfo = new InvocationInfo
            {
                AssemblyName = "ApiInspector.Test.dll",
                ClassName    = typeof(AnyClass).FullName,
                MethodName   = nameof(AnyClass.AnyMethod_2),
                Parameters = new List<InvocationMethodParameterInfo>
                {
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "\"a\""
                    },
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "5"
                    },
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "\"c\""
                    },
                    new InvocationMethodParameterInfo
                    {
                        ValueAsJson = "6"
                    }
                }
            };

            Invoke_method(invocationInfo, "a-5-c-6");
        }
        #endregion

        #region Methods
        static void Invoke_method(InvocationInfo invocationInfo, string expectedResponse)
        {
            invocationInfo.AssemblySearchDirectory = string.Empty;

            var context = new DataContext();
            context.SetupGet(Invoker.InvocationInfo,(c)=>invocationInfo);
            

            Invoker.Invoke(context);

            context.Get(Invoker.ExecutionResponse).Should().Be(expectedResponse);
        }
        #endregion
    }
}